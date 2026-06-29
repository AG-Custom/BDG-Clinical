using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Application.Modules.Abstractions;
using BGD.CLINICAL.Domain.Enums;

namespace BGD.CLINICAL.Application.Schedules.Appointments;

internal static class AppointmentScopeResolver
{
    public static async Task<Guid?> ResolveFuncionarioFilterAsync(
        ICurrentTenantContext tenantContext,
        IUsersRepository usersRepository,
        IPermissionChecker permissionChecker,
        CancellationToken cancellationToken)
    {
        var usuario = await usersRepository.GetByIdAsync(tenantContext.UsuarioId, cancellationToken);

        if (usuario is null)
        {
            return Guid.Empty;
        }

        if (usuario.TipoUsuario == TipoUsuario.Admin)
        {
            return null;
        }

        var hasEquipe = await permissionChecker.HasPermissionAsync(
            tenantContext.UsuarioId,
            "agenda.visualizar.equipe",
            cancellationToken);

        if (hasEquipe)
        {
            return null;
        }

        var hasPropria = await permissionChecker.HasPermissionAsync(
            tenantContext.UsuarioId,
            "agenda.visualizar.propria",
            cancellationToken);

        if (hasPropria)
        {
            return usuario.FuncionarioId ?? Guid.Empty;
        }

        var hasAgenda = await permissionChecker.HasPermissionAsync(
            tenantContext.UsuarioId,
            "agenda.visualizar",
            cancellationToken);

        return hasAgenda ? null : Guid.Empty;
    }
}
