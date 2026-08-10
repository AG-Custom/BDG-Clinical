namespace AG.CLINICAL.Application.Applications.Dtos;

public sealed record PatientApplicationSymptomDto(
    Guid Id,
    string Nome);

public sealed record PatientApplicationConsumedItemDto(
    Guid ProdutoId,
    string ProdutoNome,
    decimal Quantidade,
    bool ControlaEstoque,
    Guid? LoteProdutoId = null,
    string? LoteCodigo = null);

public sealed record PatientApplicationDto(
    Guid Id,
    Guid PacienteId,
    string PacienteNome,
    Guid? CompraPacienteId,
    Guid? ProdutoId,
    string? ProdutoNome,
    Guid? ProcedimentoId,
    string? ProcedimentoNome,
    Guid AplicadorId,
    string AplicadorNome,
    Guid UnidadeId,
    string UnidadeNome,
    DateTime DataAplicacao,
    decimal? QuantidadeUtilizada,
    decimal? Peso,
    string? Observacao,
    bool Realizado,
    bool Cancelada,
    IReadOnlyList<PatientApplicationSymptomDto> Sintomas,
    IReadOnlyList<PatientApplicationConsumedItemDto> ItensConsumidos,
    DateTime CriadoEm,
    DateTime? AtualizadoEm,
    Guid? LoteProdutoId = null,
    string? LoteCodigo = null);

public sealed record PatientApplicationManualSupplyRequest(
    Guid ProdutoId,
    decimal Quantidade);

public sealed record CreatePatientApplicationProcedureRequest(
    Guid ProcedimentoId,
    decimal? QuantidadeUtilizada = null,
    Guid? LoteProdutoId = null,
    bool ConsumirInsumosKit = true,
    IReadOnlyList<PatientApplicationManualSupplyRequest>? InsumosManuais = null);

public sealed record CreatePatientApplicationRequest(
    Guid PacienteId,
    Guid AplicadorId,
    Guid UnidadeId,
    DateTime DataAplicacao,
    Guid? CompraPacienteId = null,
    Guid? ProcedimentoId = null,
    decimal? QuantidadeUtilizada = null,
    Guid? LoteProdutoId = null,
    bool ConsumirInsumosKit = true,
    IReadOnlyList<PatientApplicationManualSupplyRequest>? InsumosManuais = null,
    decimal? Peso = null,
    string? Observacao = null,
    IReadOnlyList<Guid>? SintomaIds = null,
    IReadOnlyList<CreatePatientApplicationProcedureRequest>? Procedimentos = null);

public sealed record CreatePatientApplicationsResult(
    IReadOnlyList<PatientApplicationDto> Aplicacoes);

public sealed record UpdatePatientApplicationRequest(
    DateTime DataAplicacao,
    decimal? Peso = null,
    string? Observacao = null,
    IReadOnlyList<Guid>? SintomaIds = null);
