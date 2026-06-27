using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Modules.Abstractions;
using BGD.CLINICAL.Domain.Enums;
using BGD.CLINICAL.WebApi.Models.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BGD.CLINICAL.WebApi.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequireModuleLicenseAttribute : Attribute, IAsyncAuthorizationFilter
{
    public RequireModuleLicenseAttribute(string moduleCode)
    {
        ModuleCode = moduleCode;
    }

    public string ModuleCode { get; }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var licenses = context.HttpContext.RequestServices.GetRequiredService<IModuleLicensesRepository>();
        var tenant = context.HttpContext.RequestServices.GetRequiredService<ICurrentTenantContext>();

        var hasLicense = await licenses.HasActiveLicenseAsync(
            tenant.EmpresaId,
            ModuleCode,
            context.HttpContext.RequestAborted);

        if (!hasLicense)
        {
            context.Result = new ObjectResult(new ApiResponse<object?>(null!, false, "Módulo não licenciado para esta empresa."))
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }
}
