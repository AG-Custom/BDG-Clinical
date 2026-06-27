using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Modules.Abstractions;
using BGD.CLINICAL.Domain.Enums;
using BGD.CLINICAL.WebApi.Models.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BGD.CLINICAL.WebApi.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequirePermissionAttribute : Attribute, IAsyncAuthorizationFilter
{
    public RequirePermissionAttribute(string moduleCode, ModulePermissionAction? action = null)
    {
        ModuleCode = moduleCode;
        Action = action;
    }

    public string ModuleCode { get; }

    public ModulePermissionAction? Action { get; }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var permissions = context.HttpContext.RequestServices.GetRequiredService<IUserPermissionsRepository>();
        var tenant = context.HttpContext.RequestServices.GetRequiredService<ICurrentTenantContext>();

        var action = Action ?? ResolveActionFromHttpMethod(context.HttpContext.Request.Method);

        var hasPermission = await permissions.HasPermissionAsync(
            tenant.UsuarioId,
            ModuleCode,
            action,
            context.HttpContext.RequestAborted);

        if (!hasPermission)
        {
            context.Result = new ObjectResult(new ApiResponse<object?>(null!, false, "Usuário sem permissão para esta operação."))
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }

    private static ModulePermissionAction ResolveActionFromHttpMethod(string method) => method.ToUpperInvariant() switch
    {
        "GET" or "HEAD" => ModulePermissionAction.View,
        "POST" => ModulePermissionAction.Create,
        "PUT" or "PATCH" => ModulePermissionAction.Edit,
        "DELETE" => ModulePermissionAction.Delete,
        _ => ModulePermissionAction.View
    };
}
