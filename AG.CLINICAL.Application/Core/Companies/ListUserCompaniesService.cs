using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Core.Dtos;
using AG.CLINICAL.Application.Identity.Abstractions;

namespace AG.CLINICAL.Application.Core.Companies;

public interface IListUserCompaniesService
{
    Task<Result<IReadOnlyList<UserCompanyDto>>> ExecuteAsync(
        CancellationToken cancellationToken = default);
}

public sealed class ListUserCompaniesService : IListUserCompaniesService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IUsersRepository _usersRepository;

    public ListUserCompaniesService(
        ICurrentTenantContext tenantContext,
        IUsersRepository usersRepository)
    {
        _tenantContext = tenantContext;
        _usersRepository = usersRepository;
    }

    public async Task<Result<IReadOnlyList<UserCompanyDto>>> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var usuario = await _usersRepository.GetByIdAsync(_tenantContext.UsuarioId, cancellationToken);

        if (usuario is null)
        {
            return Result<IReadOnlyList<UserCompanyDto>>.Failure("Usuário não autenticado.");
        }

        var usuarios = await _usersRepository.ListByEmailLoginAsync(usuario.EmailLogin, cancellationToken);
        var companies = UserCompaniesMapper.MapMany(usuarios, _tenantContext.EmpresaId);

        return Result<IReadOnlyList<UserCompanyDto>>.Success(companies);
    }
}
