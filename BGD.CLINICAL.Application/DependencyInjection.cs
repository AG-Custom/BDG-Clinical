using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Application.Identity.Authentications;
using BGD.CLINICAL.Application.Identity.Registrations;
using BGD.CLINICAL.Application.Identity.Users;
using BGD.CLINICAL.Application.Core.Units;
using Microsoft.Extensions.DependencyInjection;

namespace BGD.CLINICAL.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ILoginUsersService, LoginUsersService>();
        services.AddScoped<IRegisterCompaniesService, RegisterCompaniesService>();
        services.AddScoped<IGetAuthenticatedUsersService, GetAuthenticatedUsersService>();

        services.AddScoped<ICreateUnitsService, CreateUnitsService>();
        services.AddScoped<IListUnitsService, ListUnitsService>();
        services.AddScoped<IGetUnitsService, GetUnitsService>();
        services.AddScoped<IUpdateUnitsService, UpdateUnitsService>();
        services.AddScoped<IDeactivateUnitsService, DeactivateUnitsService>();

        return services;
    }
}
