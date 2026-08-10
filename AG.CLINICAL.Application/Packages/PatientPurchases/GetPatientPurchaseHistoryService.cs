using System.Text.Json;
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

        foreach (var item in compra.Pacote?.Itens ?? [])
        {
            eventos.Add(new PatientPurchaseHistoryEventDto(
                Tipo: "Compra",
                Data: compra.DataCompra,
                ProdutoId: item.ProdutoId,
                ProdutoNome: item.Produto?.Nome ?? string.Empty,
                Quantidade: item.QuantidadeTotal,
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
            var motivo = ExtrairMotivo(log.DadosNovos);
            var saldoAnterior = ExtrairSaldoProdutos(log.DadosAnteriores);
            var saldoNovo = ExtrairSaldoProdutos(log.DadosNovos);

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

    private static string? ExtrairMotivo(string? dadosNovos)
    {
        if (string.IsNullOrWhiteSpace(dadosNovos))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(dadosNovos);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            if (document.RootElement.TryGetProperty("motivo", out var motivoElement)
                || document.RootElement.TryGetProperty("Motivo", out motivoElement))
            {
                return motivoElement.GetString();
            }
        }
        catch (JsonException)
        {
            return null;
        }

        return null;
    }

    private static Dictionary<Guid, ProdutoSaldoSnapshot> ExtrairSaldoProdutos(string? json)
    {
        var resultado = new Dictionary<Guid, ProdutoSaldoSnapshot>();
        if (string.IsNullOrWhiteSpace(json))
        {
            return resultado;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            JsonElement saldoElement;
            if (root.TryGetProperty("saldo", out var saldoWrapper)
                || root.TryGetProperty("Saldo", out saldoWrapper))
            {
                // Payload com motivo: { motivo, saldo: PatientPurchaseDto }
                if (saldoWrapper.TryGetProperty("Saldo", out var nested)
                    || saldoWrapper.TryGetProperty("saldo", out nested))
                {
                    saldoElement = nested;
                }
                else if (saldoWrapper.TryGetProperty("Produtos", out _)
                    || saldoWrapper.TryGetProperty("produtos", out _))
                {
                    saldoElement = saldoWrapper;
                }
                else
                {
                    return resultado;
                }
            }
            else if (root.TryGetProperty("Saldo", out var saldoDireto)
                || root.TryGetProperty("saldo", out saldoDireto))
            {
                saldoElement = saldoDireto;
            }
            else
            {
                return resultado;
            }

            if (!TryGetProperty(saldoElement, "Produtos", "produtos", out var produtosElement)
                || produtosElement.ValueKind != JsonValueKind.Array)
            {
                return resultado;
            }

            foreach (var produto in produtosElement.EnumerateArray())
            {
                if (!TryGetGuid(produto, "ProdutoId", "produtoId", out var produtoId))
                {
                    continue;
                }

                TryGetString(produto, "ProdutoNome", "produtoNome", out var produtoNome);
                TryGetString(produto, "UnidadeMedida", "unidadeMedida", out var unidadeMedida);
                TryGetDecimal(produto, "QuantidadeContratada", "quantidadeContratada", out var contratada);
                TryGetDecimal(produto, "QuantidadeUtilizada", "quantidadeUtilizada", out var utilizada);

                resultado[produtoId] = new ProdutoSaldoSnapshot(
                    produtoId,
                    produtoNome,
                    unidadeMedida,
                    contratada,
                    utilizada);
            }
        }
        catch (JsonException)
        {
            return resultado;
        }

        return resultado;
    }

    private static bool TryGetProperty(
        JsonElement element,
        string pascal,
        string camel,
        out JsonElement value)
    {
        if (element.TryGetProperty(pascal, out value) || element.TryGetProperty(camel, out value))
        {
            return true;
        }

        value = default;
        return false;
    }

    private static bool TryGetGuid(JsonElement element, string pascal, string camel, out Guid value)
    {
        value = Guid.Empty;
        if (!TryGetProperty(element, pascal, camel, out var property))
        {
            return false;
        }

        return property.TryGetGuid(out value)
            || (property.ValueKind == JsonValueKind.String
                && Guid.TryParse(property.GetString(), out value));
    }

    private static bool TryGetDecimal(
        JsonElement element,
        string pascal,
        string camel,
        out decimal value)
    {
        value = 0;
        if (!TryGetProperty(element, pascal, camel, out var property))
        {
            return false;
        }

        return property.TryGetDecimal(out value);
    }

    private static bool TryGetString(
        JsonElement element,
        string pascal,
        string camel,
        out string? value)
    {
        value = null;
        if (!TryGetProperty(element, pascal, camel, out var property))
        {
            return false;
        }

        value = property.GetString();
        return true;
    }

    private static string? ObterUnidadeMedidaAplicacao(
        CompraPaciente compra,
        AplicacaoPaciente aplicacao)
    {
        if (aplicacao.ProdutoId.HasValue)
        {
            var item = compra.Pacote?.Itens.FirstOrDefault(i => i.ProdutoId == aplicacao.ProdutoId.Value);
            if (item is not null)
            {
                return item.UnidadeMedida;
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

    private sealed record ProdutoSaldoSnapshot(
        Guid ProdutoId,
        string? ProdutoNome,
        string? UnidadeMedida,
        decimal QuantidadeContratada,
        decimal QuantidadeUtilizada);
}
