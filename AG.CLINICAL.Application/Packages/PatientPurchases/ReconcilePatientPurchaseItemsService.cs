using AG.CLINICAL.Application.Abstractions.Persistence;
using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.Application.Audits.Abstractions;
using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Packages.Abstractions;
using AG.CLINICAL.Application.Packages.Dtos;
using AG.CLINICAL.Domain.Entities;
using AG.CLINICAL.Domain.Enums;

namespace AG.CLINICAL.Application.Packages.PatientPurchases;

public interface IReconcilePatientPurchaseItemsService
{
    Task<Result<ReconcilePatientPurchaseItemsResultDto>> ExecuteAsync(
        bool dryRun,
        CancellationToken cancellationToken = default);
}

public sealed class ReconcilePatientPurchaseItemsService : IReconcilePatientPurchaseItemsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IPatientPurchasesRepository _patientPurchasesRepository;
    private readonly IAuditLogsQueryRepository _auditLogsQueryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReconcilePatientPurchaseItemsService(
        ICurrentTenantContext tenantContext,
        IPatientPurchasesRepository patientPurchasesRepository,
        IAuditLogsQueryRepository auditLogsQueryRepository,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _patientPurchasesRepository = patientPurchasesRepository;
        _auditLogsQueryRepository = auditLogsQueryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ReconcilePatientPurchaseItemsResultDto>> ExecuteAsync(
        bool dryRun,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;
        var compras = dryRun
            ? await _patientPurchasesRepository.ListByEmpresaIdAsync(
                empresaId,
                null,
                null,
                cancellationToken)
            : await _patientPurchasesRepository.ListTrackedByEmpresaIdAsync(
                empresaId,
                cancellationToken);

        var comprasPorPacote = compras
            .GroupBy(compra => compra.PacoteId)
            .ToDictionary(grupo => grupo.Key, grupo => grupo.Count());

        var relatorio = new List<ReconcilePatientPurchaseItemRowDto>();
        var comprasJaMigradas = 0;
        var comprasProcessadas = 0;
        var itensCriados = 0;

        foreach (var compra in compras)
        {
            if (compra.Itens.Count > 0)
            {
                comprasJaMigradas++;
                continue;
            }

            var itensFonte = (compra.Pacote?.Itens ?? []).ToList();
            if (itensFonte.Count == 0)
            {
                relatorio.Add(new ReconcilePatientPurchaseItemRowDto(
                    compra.PacienteId,
                    compra.Paciente?.Nome ?? string.Empty,
                    compra.Id,
                    compra.Pacote?.Nome ?? string.Empty,
                    Guid.Empty,
                    string.Empty,
                    0,
                    0,
                    0,
                    0,
                    true,
                    "Compra sem itens no pacote atual para reconstruir o contrato."));
                continue;
            }

            var logs = await _auditLogsQueryRepository.ListByEntityAsync(
                empresaId,
                nameof(CompraPaciente),
                compra.Id,
                [AcaoAuditoria.Editar],
                cancellationToken);

            var exclusivo = comprasPorPacote.GetValueOrDefault(compra.PacoteId) == 1;
            comprasProcessadas++;

            foreach (var itemFonte in itensFonte)
            {
                var aplicacoesValidas = compra.GetQuantidadeUtilizadaNasAplicacoes(itemFonte.ProdutoId);
                var (contratado, baseReconstruida, divergencia, motivo) = ReconstruirItem(
                    compra,
                    itemFonte,
                    logs,
                    exclusivo);

                var utilizada = baseReconstruida + aplicacoesValidas;
                var restante = Math.Max(0, contratado - utilizada);

                relatorio.Add(new ReconcilePatientPurchaseItemRowDto(
                    compra.PacienteId,
                    compra.Paciente?.Nome ?? string.Empty,
                    compra.Id,
                    compra.Pacote?.Nome ?? string.Empty,
                    itemFonte.ProdutoId,
                    itemFonte.Produto?.Nome ?? string.Empty,
                    contratado,
                    aplicacoesValidas,
                    baseReconstruida,
                    restante,
                    divergencia,
                    motivo));

                if (dryRun)
                {
                    continue;
                }

                compra.AdicionarItemMigrado(
                    itemFonte.ProdutoId,
                    contratado,
                    baseReconstruida,
                    itemFonte.UnidadeMedida);
                itensCriados++;
            }

            if (!dryRun && compra.Itens.Count > 0)
            {
                compra.CompleteIfExhausted();
                compra.ReopenIfCompleted();
                _patientPurchasesRepository.Update(compra);
            }
        }

        if (!dryRun)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result<ReconcilePatientPurchaseItemsResultDto>.Success(
            new ReconcilePatientPurchaseItemsResultDto(
                comprasProcessadas,
                comprasJaMigradas,
                itensCriados,
                relatorio.Count(linha => linha.Divergencia),
                !dryRun,
                relatorio));
    }

    private static (
        decimal Contratado,
        decimal BaseReconstruida,
        bool Divergencia,
        string? Motivo)
        ReconstruirItem(
            CompraPaciente compra,
            ItemPacote itemFonte,
            IReadOnlyList<EntityAuditLogRecord> logs,
            bool exclusivo)
    {
        var contratado = itemFonte.QuantidadeTotal;
        var contratadoDoAjuste = ObterUltimoValorAjuste(logs, itemFonte.ProdutoId, "quantidadeContratada");
        if (contratadoDoAjuste.HasValue && contratadoDoAjuste.Value > 0)
        {
            contratado = contratadoDoAjuste.Value;
        }

        var utilizadoDoAjuste = ObterUltimoAjusteUtilizada(logs, itemFonte.ProdutoId);
        if (utilizadoDoAjuste is { } ajusteUtilizada)
        {
            var aplicacoesNoAjuste = compra.Aplicacoes
                .Where(aplicacao =>
                    aplicacao.Realizado
                    && !aplicacao.Cancelada
                    && aplicacao.ProdutoId == itemFonte.ProdutoId
                    && aplicacao.QuantidadeUtilizada.HasValue
                    && aplicacao.DataAplicacao <= ajusteUtilizada.Data)
                .Sum(aplicacao => aplicacao.QuantidadeUtilizada!.Value);

            var baseCalculada = ajusteUtilizada.Quantidade - aplicacoesNoAjuste;
            if (baseCalculada < 0)
            {
                return (contratado, 0, true, "Ajuste manual resultaria em base negativa; conferir histórico.");
            }

            return (contratado, baseCalculada, false, null);
        }

        if (exclusivo)
        {
            return (contratado, itemFonte.QuantidadeUtilizadaBase, false, null);
        }

        if (itemFonte.QuantidadeUtilizadaBase != 0)
        {
            return (
                contratado,
                0,
                true,
                "Pacote compartilhado com base utilizada no catálogo; base da compra ficou 0 para revisão.");
        }

        return (contratado, 0, false, null);
    }

    private static decimal? ObterUltimoValorAjuste(
        IReadOnlyList<EntityAuditLogRecord> logs,
        Guid produtoId,
        string campo)
    {
        foreach (var log in logs.OrderByDescending(item => item.Data))
        {
            var saldoNovo = PatientPurchaseSaldoAuditParser.ExtrairSaldoProdutos(log.DadosNovos);
            var saldoAnterior = PatientPurchaseSaldoAuditParser.ExtrairSaldoProdutos(log.DadosAnteriores);
            if (!saldoNovo.TryGetValue(produtoId, out var novo))
            {
                continue;
            }

            saldoAnterior.TryGetValue(produtoId, out var anterior);
            if (campo == "quantidadeContratada"
                && (anterior is null || anterior.QuantidadeContratada != novo.QuantidadeContratada))
            {
                return novo.QuantidadeContratada;
            }
        }

        return null;
    }

    private static (DateTime Data, decimal Quantidade)? ObterUltimoAjusteUtilizada(
        IReadOnlyList<EntityAuditLogRecord> logs,
        Guid produtoId)
    {
        foreach (var log in logs.OrderByDescending(item => item.Data))
        {
            var saldoNovo = PatientPurchaseSaldoAuditParser.ExtrairSaldoProdutos(log.DadosNovos);
            var saldoAnterior = PatientPurchaseSaldoAuditParser.ExtrairSaldoProdutos(log.DadosAnteriores);
            if (!saldoNovo.TryGetValue(produtoId, out var novo))
            {
                continue;
            }

            saldoAnterior.TryGetValue(produtoId, out var anterior);
            if (anterior is null || anterior.QuantidadeUtilizada != novo.QuantidadeUtilizada)
            {
                return (log.Data, novo.QuantidadeUtilizada);
            }
        }

        return null;
    }
}
