using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Identity.Abstractions;
using AG.CLINICAL.Application.Identity.Dtos;

namespace AG.CLINICAL.Application.Identity.Users;

public interface IResolveUserDisplayNamesService
{
    Task<Result<IReadOnlyList<UserDisplayNameDto>>> ExecuteAsync(
        IReadOnlyList<Guid> ids,
        CancellationToken cancellationToken = default);
}

public sealed class ResolveUserDisplayNamesService : IResolveUserDisplayNamesService
{
    private const int MaxIds = 20;

    private readonly ICurrentTenantContext _tenantContext;
    private readonly IUsersRepository _usersRepository;

    public ResolveUserDisplayNamesService(
        ICurrentTenantContext tenantContext,
        IUsersRepository usersRepository)
    {
        _tenantContext = tenantContext;
        _usersRepository = usersRepository;
    }

    public async Task<Result<IReadOnlyList<UserDisplayNameDto>>> ExecuteAsync(
        IReadOnlyList<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        if (ids is null || ids.Count == 0)
        {
            return Result<IReadOnlyList<UserDisplayNameDto>>.Success(Array.Empty<UserDisplayNameDto>());
        }

        var distinctIds = ids
            .Where(id => id != Guid.Empty)
            .Distinct()
            .Take(MaxIds)
            .ToList();

        if (distinctIds.Count == 0)
        {
            return Result<IReadOnlyList<UserDisplayNameDto>>.Success(Array.Empty<UserDisplayNameDto>());
        }

        var usuarios = await _usersRepository.ListDisplayNamesByIdsAndEmpresaIdAsync(
            _tenantContext.EmpresaId,
            distinctIds,
            cancellationToken);

        var dtos = usuarios
            .Select(usuario => new UserDisplayNameDto(usuario.Id, usuario.Nome))
            .ToList();

        return Result<IReadOnlyList<UserDisplayNameDto>>.Success(dtos);
    }
}
