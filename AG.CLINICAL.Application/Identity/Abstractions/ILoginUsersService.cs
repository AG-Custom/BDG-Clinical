using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Identity.Dtos;

namespace AG.CLINICAL.Application.Identity.Abstractions;

public interface ILoginUsersService
{
    Task<Result<LoginResponse>> ExecuteAsync(
        LoginRequest request,
        string? ip,
        CancellationToken cancellationToken = default);
}
