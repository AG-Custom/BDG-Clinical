namespace BGD.CLINICAL.Application.Applications.Dtos;

public sealed record PatientApplicationSymptomDto(
    Guid Id,
    string Nome);

public sealed record PatientApplicationDto(
    Guid Id,
    Guid PacienteId,
    string PacienteNome,
    Guid? CompraPacienteId,
    Guid ProdutoId,
    string ProdutoNome,
    Guid AplicadorId,
    string AplicadorNome,
    Guid UnidadeId,
    string UnidadeNome,
    DateTime DataAplicacao,
    decimal QuantidadeUtilizada,
    decimal? Peso,
    string? Observacao,
    bool Realizado,
    bool Cancelada,
    IReadOnlyList<PatientApplicationSymptomDto> Sintomas,
    DateTime CriadoEm,
    DateTime? AtualizadoEm);

public sealed record CreatePatientApplicationRequest(
    Guid PacienteId,
    Guid ProdutoId,
    Guid AplicadorId,
    Guid UnidadeId,
    decimal QuantidadeUtilizada,
    DateTime DataAplicacao,
    decimal? Peso = null,
    string? Observacao = null,
    IReadOnlyList<Guid>? SintomaIds = null,
    Guid? CompraPacienteId = null);

public sealed record UpdatePatientApplicationRequest(
    DateTime DataAplicacao,
    decimal? Peso = null,
    string? Observacao = null,
    IReadOnlyList<Guid>? SintomaIds = null);
