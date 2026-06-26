namespace BGD.CLINICAL.Application.Patients.Dtos;

public sealed record SymptomDto(
    Guid Id,
    string Nome,
    bool Ativo,
    DateTime CriadoEm,
    DateTime? AtualizadoEm);

public sealed record CreateSymptomRequest(string Nome);

public sealed record UpdateSymptomRequest(string Nome);
