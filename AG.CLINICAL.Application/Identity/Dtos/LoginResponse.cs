using AG.CLINICAL.Application.Core.Dtos;

namespace AG.CLINICAL.Application.Identity.Dtos;

public sealed record LoginResponse(
    bool RequiresCompanySelection,
    string? Token,
    AuthenticatedUserDto? Usuario,
    IReadOnlyList<UserCompanyDto>? Companies);
