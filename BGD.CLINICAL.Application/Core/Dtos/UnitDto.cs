namespace BGD.CLINICAL.Application.Core.Dtos;

public sealed record UnitDto(
    Guid Id,
    string Nome,
    string? Endereco,
    bool Ativo,
    DateTime CriadoEm,
    DateTime? AtualizadoEm);
