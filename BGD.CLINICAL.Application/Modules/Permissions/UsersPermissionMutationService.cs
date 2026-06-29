using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Application.Modules.Abstractions;

namespace BGD.CLINICAL.Application.Modules.Permissions;

public interface IUsersPermissionMutationService
{
    Task InvalidateUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default);

    Task InvalidateUsuariosAsync(IReadOnlyList<Guid> usuarioIds, CancellationToken cancellationToken = default);
}

public sealed class UsersPermissionMutationService : IUsersPermissionMutationService
{
    private readonly IUsersRepository _usersRepository;
    private readonly IPermissionCacheInvalidator _cacheInvalidator;
    private readonly IUnitOfWork _unitOfWork;

    public UsersPermissionMutationService(
        IUsersRepository usersRepository,
        IPermissionCacheInvalidator cacheInvalidator,
        IUnitOfWork unitOfWork)
    {
        _usersRepository = usersRepository;
        _cacheInvalidator = cacheInvalidator;
        _unitOfWork = unitOfWork;
    }

    public async Task InvalidateUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var usuario = await _usersRepository.GetByIdAsync(usuarioId, cancellationToken);

        if (usuario is null)
        {
            return;
        }

        usuario.IncrementPermissionVersion();
        _usersRepository.Update(usuario);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _cacheInvalidator.InvalidateUsuario(usuarioId);
    }

    public async Task InvalidateUsuariosAsync(IReadOnlyList<Guid> usuarioIds, CancellationToken cancellationToken = default)
    {
        foreach (var usuarioId in usuarioIds.Distinct())
        {
            await InvalidateUsuarioAsync(usuarioId, cancellationToken);
        }
    }
}
