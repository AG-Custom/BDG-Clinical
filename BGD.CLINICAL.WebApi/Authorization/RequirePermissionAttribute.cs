using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Modules.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BGD.CLINICAL.WebApi.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequirePermissionAttribute : Attribute, IAsyncAuthorizationFilter
{
    public RequirePermissionAttribute(string permissionKey)
    {
        PermissionKey = permissionKey;
    }

    public string PermissionKey { get; }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var tenant = context.HttpContext.RequestServices.GetRequiredService<ICurrentTenantContext>();
        var permissionChecker = context.HttpContext.RequestServices.GetRequiredService<IPermissionChecker>();
        var licenses = context.HttpContext.RequestServices.GetRequiredService<IModuleLicensesRepository>();
        var usersRepository = context.HttpContext.RequestServices.GetRequiredService<Application.Identity.Abstractions.IUsersRepository>();

        if (await PermissionAuthorizationHelper.IsAdminAsync(
                tenant,
                usersRepository,
                context.HttpContext.RequestAborted))
        {
            return;
        }

        var hasPermission = await PermissionAuthorizationHelper.HasLicensedPermissionAsync(
            tenant,
            permissionChecker,
            licenses,
            PermissionKey,
            context.HttpContext.RequestAborted);

        if (!hasPermission)
        {
            context.Result = PermissionAuthorizationHelper.Forbidden(
                "Usuário sem permissão para esta operação.");
        }
    }
}
