namespace BGD.CLINICAL.Application.Patients.Dtos;

public sealed record PatientDto(
    Guid Id,
    Guid UnidadeId,
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
    Guid UnidadeId,
    string Nome,
    string? Cpf,
    string? Telefone,
    string? Email,
    DateOnly? DataNascimento,
    string? Observacao);

public sealed record UpdatePatientRequest(
    Guid UnidadeId,
    string Nome,
    string? Cpf,
    string? Telefone,
    string? Email,
    DateOnly? DataNascimento,
    string? Observacao);
