namespace BGD.CLINICAL.Application.Patients.Dtos;

public sealed record PatientUnitDto(Guid Id, string Nome);

public sealed record PatientDto(
    Guid Id,
    Guid UnidadeId,
    IReadOnlyList<PatientUnitDto> Unidades,
    IReadOnlyList<Guid> UnidadeIds,
    string Nome,
    string? Cpf,
    string? Telefone,
    string? Email,
    DateOnly? DataNascimento,
    string? Observacao,
    bool Ativo,
    DateTime CriadoEm,
    DateTime? AtualizadoEm);

public sealed record CreatePatientRequest(
    string Nome,
    Guid? UnidadeId = null,
    IReadOnlyList<Guid>? UnidadeIds = null,
    string? Cpf = null,
    string? Telefone = null,
    string? Email = null,
    DateOnly? DataNascimento = null,
    string? Observacao = null);

public sealed record UpdatePatientRequest(
    string Nome,
    Guid? UnidadeId = null,
    IReadOnlyList<Guid>? UnidadeIds = null,
    string? Cpf = null,
    string? Telefone = null,
    string? Email = null,
    DateOnly? DataNascimento = null,
    string? Observacao = null);
