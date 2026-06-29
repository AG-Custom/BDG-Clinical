namespace BGD.CLINICAL.Application.Core.Dtos;

public sealed record PositionDto(
    Guid Id,
    string Nome,
    bool FlagAplicador,
    bool Ativo,
    DateTime CriadoEm,
    DateTime? AtualizadoEm);

public sealed record CreatePositionRequest(string Nome, bool FlagAplicador = false);

public sealed record UpdatePositionRequest(string Nome, bool FlagAplicador);
