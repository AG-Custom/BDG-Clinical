using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Identity.Dtos;
using System.Security.Claims;

namespace AG.CLINICAL.Application.Identity.Abstractions;

public interface IGetAuthenticatedUsersService
{
    Task<Result<AuthenticatedUserDto>> ExecuteAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default);
}
