using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.Application.Identity.Abstractions;
using AG.CLINICAL.Application.Modules.Abstractions;
using AG.CLINICAL.Application.Modules.Permissions;
using AG.CLINICAL.Domain.Enums;
using AG.CLINICAL.WebApi.Models.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AG.CLINICAL.WebApi.Authorization;

internal static class PermissionAuthorizationHelper
{
    public static async Task<bool> IsAdminAsync(
        ICurrentTenantContext tenant,
        IUsersRepository usersRepository,
        CancellationToken cancellationToken)
    {
        var usuario = await usersRepository.GetByIdAsync(tenant.UsuarioId, cancellationToken);
        return usuario is not null && usuario.TipoUsuario == TipoUsuario.Admin;
    }

    public static async Task<bool> HasLicensedPermissionAsync(
        ICurrentTenantContext tenant,
        IPermissionChecker permissionChecker,
        IModuleLicensesRepository licenses,
        string permissionKey,
        CancellationToken cancellationToken)
    {
        var moduleCode = ResolveModuleCode(permissionKey);

        if (moduleCode is not null
            && !string.Equals(moduleCode, PermissionModuleCodes.Core, StringComparison.OrdinalIgnoreCase))
        {
            var hasLicense = await licenses.HasActiveLicenseAsync(
                tenant.EmpresaId,
                moduleCode,
                cancellationToken);

            if (!hasLicense)
            {
                return false;
            }
        }

        return await permissionChecker.HasPermissionAsync(
            tenant.UsuarioId,
            permissionKey,
            cancellationToken);
    }

    public static async Task<bool> HasAnyLicensedPermissionAsync(
        ICurrentTenantContext tenant,
        IPermissionChecker permissionChecker,
        IModuleLicensesRepository licenses,
        IReadOnlyList<string> permissionKeys,
        CancellationToken cancellationToken)
    {
        foreach (var permissionKey in permissionKeys)
        {
            if (await HasLicensedPermissionAsync(
                    tenant,
                    permissionChecker,
                    licenses,
                    permissionKey,
                    cancellationToken))
            {
                return true;
            }
        }

        return false;
    }

    public static string? ResolveModuleCode(string permissionKey)
    {
        var definition = PermissionCatalog.All
            .FirstOrDefault(item => string.Equals(item.Key, permissionKey, StringComparison.OrdinalIgnoreCase));

        if (definition is not null)
        {
            return definition.Module;
        }

        var resourcePrefix = permissionKey.Split('.')[0];

        return PermissionCatalog.All
            .FirstOrDefault(item => item.Key.StartsWith(resourcePrefix + ".", StringComparison.OrdinalIgnoreCase))
            ?.Module;
    }

    public static ObjectResult Forbidden(string message)
    {
        return new ObjectResult(new ApiResponse<object?>(null!, false, message))
        {
            StatusCode = StatusCodes.Status403Forbidden
        };
    }
}
