using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.Application.Modules.Abstractions;
using AG.CLINICAL.Domain.Enums;
using AG.CLINICAL.WebApi.Models.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AG.CLINICAL.WebApi.Authorization;

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
