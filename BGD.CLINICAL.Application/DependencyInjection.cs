using BGD.CLINICAL.Application.Abstractions.Identity;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Application.Identity.Authentications;
using BGD.CLINICAL.Application.Identity.Registrations;
using BGD.CLINICAL.Application.Identity.FirstAccess;
using BGD.CLINICAL.Application.Identity.Users;
using BGD.CLINICAL.Application.Core.Units;
using BGD.CLINICAL.Application.Core.Companies;
using BGD.CLINICAL.Application.Core.Employees;
using BGD.CLINICAL.Application.Core.Positions;
using Microsoft.Extensions.DependencyInjection;

namespace BGD.CLINICAL.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ILoginUsersService, LoginUsersService>();
        services.AddScoped<ISwitchCompanyService, SwitchCompanyService>();
        services.AddScoped<IRegisterCompaniesService, RegisterCompaniesService>();
        services.AddScoped<IGetAuthenticatedUsersService, GetAuthenticatedUsersService>();
        services.AddScoped<IProvisionEmployeeUsersService, ProvisionEmployeeUsersService>();

        services.AddScoped<IEmployeeFirstAccessInvitationService, EmployeeFirstAccessInvitationService>();
        services.AddScoped<IValidateFirstAccessEmailsService, ValidateFirstAccessEmailsService>();
        services.AddScoped<ICompleteFirstAccessService, CompleteFirstAccessService>();

        services.AddScoped<ICreateUnitsService, CreateUnitsService>();
        services.AddScoped<IListUnitsService, ListUnitsService>();
        services.AddScoped<IGetUnitsService, GetUnitsService>();
        services.AddScoped<IUpdateUnitsService, UpdateUnitsService>();
        services.AddScoped<IDeactivateUnitsService, DeactivateUnitsService>();
        services.AddScoped<IReactivateUnitsService, ReactivateUnitsService>();

        services.AddScoped<ICreatePositionsService, CreatePositionsService>();
        services.AddScoped<IListPositionsService, ListPositionsService>();
        services.AddScoped<IGetPositionsService, GetPositionsService>();
        services.AddScoped<IUpdatePositionsService, UpdatePositionsService>();
        services.AddScoped<IDeactivatePositionsService, DeactivatePositionsService>();
        services.AddScoped<IReactivatePositionsService, ReactivatePositionsService>();

        services.AddScoped<IGetCurrentCompanyService, GetCurrentCompanyService>();
        services.AddScoped<IListUserCompaniesService, ListUserCompaniesService>();
        services.AddScoped<ICreateCompanyService, CreateCompanyService>();
        services.AddScoped<IUpdateCurrentCompanyService, UpdateCurrentCompanyService>();
        services.AddScoped<IReactivateCompanyService, ReactivateCompanyService>();
        services.AddScoped<IUploadCompanyLogoService, UploadCompanyLogoService>();

        services.AddScoped<ICreateEmployeesService, CreateEmployeesService>();
        services.AddScoped<IListEmployeesService, ListEmployeesService>();
        services.AddScoped<IGetEmployeesService, GetEmployeesService>();
        services.AddScoped<IUpdateEmployeesService, UpdateEmployeesService>();
        services.AddScoped<IDeactivateEmployeesService, DeactivateEmployeesService>();
        services.AddScoped<IReactivateEmployeesService, ReactivateEmployeesService>();

        return services;
    }
}
