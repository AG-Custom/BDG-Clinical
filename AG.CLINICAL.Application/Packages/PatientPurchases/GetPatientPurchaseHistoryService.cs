using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.Application.Applications.Abstractions;
using AG.CLINICAL.Application.Audits.Abstractions;
using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Identity.Abstractions;
using AG.CLINICAL.Application.Packages.Abstractions;
using AG.CLINICAL.Application.Packages.Dtos;
using AG.CLINICAL.Domain.Entities;
using AG.CLINICAL.Domain.Enums;

namespace AG.CLINICAL.Application.Packages.PatientPurchases;

public interface IGetPatientPurchaseHistoryService
{
    Task<Result<PatientPurchaseHistoryDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

public sealed class GetPatientPurchaseHistoryService : IGetPatientPurchaseHistoryService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IPatientPurchasesRepository _patientPurchasesRepository;
    private readonly IPatientApplicationsRepository _patientApplicationsRepository;
    private readonly IAuditLogsQueryRepository _auditLogsQueryRepository;
    private readonly IUsersRepository _usersRepository;

    public GetPatientPurchaseHistoryService(
        ICurrentTenantContext tenantContext,
        IPatientPurchasesRepository patientPurchasesRepository,
        IPatientApplicationsRepository patientApplicationsRepository,
        IAuditLogsQueryRepository auditLogsQueryRepository,
        IUsersRepository usersRepository)
    {
        _tenantContext = tenantContext;
        _patientPurchasesRepository = patientPurchasesRepository;
        _patientApplicationsRepository = patientApplicationsRepository;
        _auditLogsQueryRepository = auditLogsQueryRepository;
        _usersRepository = usersRepository;
    }

    public async Task<Result<PatientPurchaseHistoryDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;

        var compra = await _patientPurchasesRepository.GetByIdAndEmpresaIdWithDetailsAsync(
            id,
            empresaId,
            cancellationToken);

        if (compra is null)
        {
            return Result<PatientPurchaseHistoryDto>.Failure("Compra de pacote não encontrada.");
        }

        var aplicacoes = await _patientApplicationsRepository.ListByCompraPacienteIdWithDetailsAsync(
            empresaId,
            id,
            cancellationToken);

        var logs = await _auditLogsQueryRepository.ListByEntityAsync(
            empresaId,
            nameof(CompraPaciente),
            id,
            [
                AcaoAuditoria.Criar,
                AcaoAuditoria.Editar,
                AcaoAuditoria.Cancelar,
            ],
            cancellationToken);

        var usuarioIds = logs
            .Select(log => log.UsuarioId)
            .Distinct()
            .ToList();

        var nomesUsuarios = await _usersRepository.ListDisplayNamesByIdsAndEmpresaIdAsync(
            empresaId,
            usuarioIds,
            cancellationToken);

        var nomePorUsuarioId = nomesUsuarios.ToDictionary(item => item.Id, item => item.Nome);

        var eventos = new List<PatientPurchaseHistoryEventDto>();

        AdicionarEventosCompra(compra, logs, nomePorUsuarioId, eventos);
        AdicionarEventosAplicacoes(compra, aplicacoes, eventos);
        AdicionarEventosAjusteManual(logs, nomePorUsuarioId, eventos);
        AdicionarEventosCancelamentoCompra(compra, logs, nomePorUsuarioId, eventos);

        var ordenados = eventos
            .OrderBy(evento => evento.Data)
            .ThenBy(evento => evento.Tipo)
            .ToList();

        return Result<PatientPurchaseHistoryDto>.Success(
            new PatientPurchaseHistoryDto(compra.Id, ordenados));
    }

    private static void AdicionarEventosCompra(
        CompraPaciente compra,
        IReadOnlyList<EntityAuditLogRecord> logs,
        IReadOnlyDictionary<Guid, string> nomePorUsuarioId,
        List<PatientPurchaseHistoryEventDto> eventos)
    {
        var logCriacao = logs
            .Where(log => log.Acao == AcaoAuditoria.Criar)
            .OrderBy(log => log.Data)
            .FirstOrDefault();

        Guid? usuarioId = logCriacao?.UsuarioId;
        string? usuarioNome = usuarioId.HasValue && nomePorUsuarioId.TryGetValue(usuarioId.Value, out var nome)
            ? nome
            : null;

        foreach (var item in ListarItensHistoricoCompra(compra))
        {
            eventos.Add(new PatientPurchaseHistoryEventDto(
                Tipo: "Compra",
                Data: compra.DataCompra,
                ProdutoId: item.ProdutoId,
                ProdutoNome: item.ProdutoNome,
                Quantidade: item.Quantidade,
                UnidadeMedida: item.UnidadeMedida,
                UsuarioId: usuarioId,
                UsuarioNome: usuarioNome));
        }
    }

    private static void AdicionarEventosAplicacoes(
        CompraPaciente compra,
        IReadOnlyList<AplicacaoPaciente> aplicacoes,
        List<PatientPurchaseHistoryEventDto> eventos)
    {
        foreach (var aplicacao in aplicacoes)
        {
            var lote = ObterLoteProdutoAplicado(aplicacao);
            var unidadeMedida = ObterUnidadeMedidaAplicacao(compra, aplicacao);

            if (!aplicacao.Cancelada)
            {
                eventos.Add(new PatientPurchaseHistoryEventDto(
                    Tipo: "Aplicacao",
                    Data: aplicacao.DataAplicacao,
                    ProdutoId: aplicacao.ProdutoId,
                    ProdutoNome: aplicacao.Produto?.Nome,
                    Quantidade: aplicacao.QuantidadeUtilizada,
                    UnidadeMedida: unidadeMedida,
                    AplicadorId: aplicacao.FuncionarioId,
                    AplicadorNome: aplicacao.Funcionario?.Nome ?? string.Empty,
                    LoteProdutoId: lote?.LoteProdutoId,
                    LoteCodigo: lote?.LoteCodigo,
                    AplicacaoId: aplicacao.Id,
                    Cancelada: false));
                continue;
            }

            eventos.Add(new PatientPurchaseHistoryEventDto(
                Tipo: "Aplicacao",
                Data: aplicacao.DataAplicacao,
                ProdutoId: aplicacao.ProdutoId,
                ProdutoNome: aplicacao.Produto?.Nome,
                Quantidade: aplicacao.QuantidadeUtilizada,
                UnidadeMedida: unidadeMedida,
                AplicadorId: aplicacao.FuncionarioId,
                AplicadorNome: aplicacao.Funcionario?.Nome ?? string.Empty,
                LoteProdutoId: lote?.LoteProdutoId,
                LoteCodigo: lote?.LoteCodigo,
                AplicacaoId: aplicacao.Id,
                Cancelada: true));

            eventos.Add(new PatientPurchaseHistoryEventDto(
                Tipo: "CancelamentoAplicacao",
                Data: aplicacao.AtualizadoEm ?? aplicacao.DataAplicacao,
                ProdutoId: aplicacao.ProdutoId,
                ProdutoNome: aplicacao.Produto?.Nome,
                Quantidade: aplicacao.QuantidadeUtilizada,
                UnidadeMedida: unidadeMedida,
                AplicadorId: aplicacao.FuncionarioId,
                AplicadorNome: aplicacao.Funcionario?.Nome ?? string.Empty,
                LoteProdutoId: lote?.LoteProdutoId,
                LoteCodigo: lote?.LoteCodigo,
                AplicacaoId: aplicacao.Id,
                Cancelada: true));
        }
    }

    private static void AdicionarEventosAjusteManual(
        IReadOnlyList<EntityAuditLogRecord> logs,
        IReadOnlyDictionary<Guid, string> nomePorUsuarioId,
        List<PatientPurchaseHistoryEventDto> eventos)
    {
        foreach (var log in logs.Where(item => item.Acao == AcaoAuditoria.Editar))
        {
            var motivo = PatientPurchaseSaldoAuditParser.ExtrairMotivo(log.DadosNovos);
            var saldoAnterior = PatientPurchaseSaldoAuditParser.ExtrairSaldoProdutos(log.DadosAnteriores);
            var saldoNovo = PatientPurchaseSaldoAuditParser.ExtrairSaldoProdutos(log.DadosNovos);

            string? usuarioNome = nomePorUsuarioId.TryGetValue(log.UsuarioId, out var nome)
                ? nome
                : null;

            var produtos = saldoNovo.Keys
                .Union(saldoAnterior.Keys)
                .Distinct()
                .ToList();

            if (produtos.Count == 0)
            {
                eventos.Add(new PatientPurchaseHistoryEventDto(
                    Tipo: "AjusteManual",
                    Data: log.Data,
                    UsuarioId: log.UsuarioId,
                    UsuarioNome: usuarioNome,
                    Motivo: motivo));
                continue;
            }

            foreach (var produtoId in produtos)
            {
                saldoAnterior.TryGetValue(produtoId, out var anterior);
                saldoNovo.TryGetValue(produtoId, out var novo);

                if (anterior is not null
                    && novo is not null
                    && anterior.QuantidadeContratada == novo.QuantidadeContratada
                    && anterior.QuantidadeUtilizada == novo.QuantidadeUtilizada)
                {
                    continue;
                }

                if (anterior?.QuantidadeContratada != novo?.QuantidadeContratada)
                {
                    eventos.Add(new PatientPurchaseHistoryEventDto(
                        Tipo: "AjusteManual",
                        Data: log.Data,
                        ProdutoId: produtoId,
                        ProdutoNome: novo?.ProdutoNome ?? anterior?.ProdutoNome,
                        QuantidadeAnterior: anterior?.QuantidadeContratada,
                        QuantidadeNova: novo?.QuantidadeContratada,
                        CampoAjuste: "quantidadeContratada",
                        UnidadeMedida: novo?.UnidadeMedida ?? anterior?.UnidadeMedida,
                        UsuarioId: log.UsuarioId,
                        UsuarioNome: usuarioNome,
                        Motivo: motivo));
                }

                if (anterior?.QuantidadeUtilizada != novo?.QuantidadeUtilizada)
                {
                    eventos.Add(new PatientPurchaseHistoryEventDto(
                        Tipo: "AjusteManual",
                        Data: log.Data,
                        ProdutoId: produtoId,
                        ProdutoNome: novo?.ProdutoNome ?? anterior?.ProdutoNome,
                        QuantidadeAnterior: anterior?.QuantidadeUtilizada,
                        QuantidadeNova: novo?.QuantidadeUtilizada,
                        CampoAjuste: "quantidadeUtilizada",
                        UnidadeMedida: novo?.UnidadeMedida ?? anterior?.UnidadeMedida,
                        UsuarioId: log.UsuarioId,
                        UsuarioNome: usuarioNome,
                        Motivo: motivo));
                }
            }
        }
    }

    private static void AdicionarEventosCancelamentoCompra(
        CompraPaciente compra,
        IReadOnlyList<EntityAuditLogRecord> logs,
        IReadOnlyDictionary<Guid, string> nomePorUsuarioId,
        List<PatientPurchaseHistoryEventDto> eventos)
    {
        if (compra.Status != StatusCompraPaciente.Cancelado)
        {
            return;
        }

        var logCancelamento = logs
            .Where(log => log.Acao == AcaoAuditoria.Cancelar)
            .OrderByDescending(log => log.Data)
            .FirstOrDefault();

        Guid? usuarioId = logCancelamento?.UsuarioId;
        string? usuarioNome = usuarioId.HasValue && nomePorUsuarioId.TryGetValue(usuarioId.Value, out var nome)
            ? nome
            : null;

        eventos.Add(new PatientPurchaseHistoryEventDto(
            Tipo: "CancelamentoCompra",
            Data: logCancelamento?.Data ?? compra.AtualizadoEm ?? compra.CriadoEm,
            UsuarioId: usuarioId,
            UsuarioNome: usuarioNome,
            Motivo: compra.Observacao));
    }

    private static IEnumerable<(Guid ProdutoId, string ProdutoNome, decimal Quantidade, string UnidadeMedida)>
        ListarItensHistoricoCompra(CompraPaciente compra)
    {
        if (compra.Itens.Count > 0)
        {
            return compra.Itens.Select(item => (
                item.ProdutoId,
                item.Produto?.Nome ?? string.Empty,
                item.QuantidadeContratada,
                item.UnidadeMedida));
        }

        return (compra.Pacote?.Itens ?? []).Select(item => (
            item.ProdutoId,
            item.Produto?.Nome ?? string.Empty,
            item.QuantidadeTotal,
            item.UnidadeMedida));
    }

    private static string? ObterUnidadeMedidaAplicacao(
        CompraPaciente compra,
        AplicacaoPaciente aplicacao)
    {
        if (aplicacao.ProdutoId.HasValue)
        {
            var unidade = compra.ObterUnidadeMedida(aplicacao.ProdutoId.Value);
            if (!string.IsNullOrWhiteSpace(unidade))
            {
                return unidade;
            }
        }

        return aplicacao.Produto?.UnidadeMedida?.Sigla
            ?? aplicacao.Produto?.UnidadeMedida?.Nome;
    }

    private static (Guid? LoteProdutoId, string? LoteCodigo)? ObterLoteProdutoAplicado(
        AplicacaoPaciente aplicacao)
    {
        if (!aplicacao.ProdutoId.HasValue)
        {
            return null;
        }

        var movimentacao = aplicacao.MovimentacoesEstoque
            .Where(item =>
                item.Tipo == TipoMovimentacaoEstoque.Saida
                && item.ProdutoId == aplicacao.ProdutoId.Value
                && item.LoteProdutoId.HasValue)
            .OrderBy(item => item.CriadoEm)
            .FirstOrDefault();

        if (movimentacao is null)
        {
            return null;
        }

        return (movimentacao.LoteProdutoId, movimentacao.LoteProduto?.Codigo);
    }
}
