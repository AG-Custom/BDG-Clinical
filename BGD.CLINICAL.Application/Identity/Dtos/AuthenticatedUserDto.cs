namespace BGD.CLINICAL.Application.Identity.Dtos;

public sealed record AuthenticatedUserDto(
    Guid Id,
    string Nome,
    string Email,
    bool IsAdmin,
    bool FlagAplicador);
