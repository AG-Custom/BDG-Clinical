using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Application.Identity.Dtos;
using BGD.CLINICAL.Application.Modules.Abstractions;
using BGD.CLINICAL.Domain.Enums;
using System.Security.Claims;

namespace BGD.CLINICAL.Application.Identity.Users;

public sealed class GetAuthenticatedUsersService : IGetAuthenticatedUsersService
{
    private readonly IUsersRepository _usersRepository;
    private readonly IPermissionChecker _permissionChecker;

    public GetAuthenticatedUsersService(
        IUsersRepository usersRepository,
        IPermissionChecker permissionChecker)
    {
        _usersRepository = usersRepository;
        _permissionChecker = permissionChecker;
    }

    public async Task<Result<AuthenticatedUserDto>> ExecuteAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        var usuarioIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? principal.FindFirst("sub")?.Value;

        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
        {
            return Result<AuthenticatedUserDto>.Failure("Usuário não autenticado.");
        }

        var usuario = await _usersRepository.GetByIdAsync(usuarioId, cancellationToken);

        if (usuario is null || !usuario.Ativo)
        {
            return Result<AuthenticatedUserDto>.Failure("Usuário não autenticado.");
        }

        if (!usuario.Empresa.Ativo && usuario.TipoUsuario != TipoUsuario.Admin)
        {
            return Result<AuthenticatedUserDto>.Failure("Usuário não autenticado.");
        }

        var permissions = await _permissionChecker.GetEffectivePermissionsAsync(usuarioId, cancellationToken);

        return Result<AuthenticatedUserDto>.Success(
            AuthenticatedUsersMapper.Map(usuario, permissions.OrderBy(key => key).ToList()));
    }
}
