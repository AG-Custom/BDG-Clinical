namespace BGD.CLINICAL.Application.Core.Dtos;

public sealed record PositionDto(
    Guid Id,
    string Nome,
    bool Ativo,
    DateTime CriadoEm,
    DateTime? AtualizadoEm);

public sealed record CreatePositionRequest(string Nome);

public sealed record UpdatePositionRequest(string Nome);
