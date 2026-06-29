using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Modules.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BGD.CLINICAL.WebApi.Authorization;

/// <summary>
/// Aceita qualquer permissão de um conjunto pré-definido em <see cref="AuxiliaryPermissionAlternates"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class RequireAnyPermissionFromAttribute : Attribute, IAsyncAuthorizationFilter
{
    public RequireAnyPermissionFromAttribute(AuxiliaryPermissionSet set)
    {
        Set = set;
    }

    public AuxiliaryPermissionSet Set { get; }

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

        var hasPermission = await PermissionAuthorizationHelper.HasAnyLicensedPermissionAsync(
            tenant,
            permissionChecker,
            licenses,
            ResolveKeys(Set),
            context.HttpContext.RequestAborted);

        if (!hasPermission)
        {
            context.Result = PermissionAuthorizationHelper.Forbidden(
                "Usuário sem permissão para esta operação.");
        }
    }

    private static string[] ResolveKeys(AuxiliaryPermissionSet set) =>
        set switch
        {
            AuxiliaryPermissionSet.Units => AuxiliaryPermissionAlternates.Units,
            AuxiliaryPermissionSet.Employees => AuxiliaryPermissionAlternates.Employees,
            AuxiliaryPermissionSet.OperatingHours => AuxiliaryPermissionAlternates.OperatingHours,
            AuxiliaryPermissionSet.Patients => AuxiliaryPermissionAlternates.Patients,
            AuxiliaryPermissionSet.Procedures => AuxiliaryPermissionAlternates.Procedures,
            AuxiliaryPermissionSet.Positions => AuxiliaryPermissionAlternates.Positions,
            AuxiliaryPermissionSet.ProductTypes => AuxiliaryPermissionAlternates.ProductTypes,
            AuxiliaryPermissionSet.MeasurementUnits => AuxiliaryPermissionAlternates.MeasurementUnits,
            AuxiliaryPermissionSet.Products => AuxiliaryPermissionAlternates.Products,
            AuxiliaryPermissionSet.Suppliers => AuxiliaryPermissionAlternates.Suppliers,
            AuxiliaryPermissionSet.Symptoms => AuxiliaryPermissionAlternates.Symptoms,
            _ => throw new ArgumentOutOfRangeException(nameof(set), set, null)
        };
}

public enum AuxiliaryPermissionSet
{
    Units,
    Employees,
    OperatingHours,
    Patients,
    Procedures,
    Positions,
    ProductTypes,
    MeasurementUnits,
    Products,
    Suppliers,
    Symptoms
}
