using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Identity.Dtos;

namespace AG.CLINICAL.Application.Identity.Abstractions;

public interface IRegisterCompaniesService
{
    Task<Result<AuthResponse>> ExecuteAsync(
        RegisterCompanyRequest request,
        CancellationToken cancellationToken = default);
}
