using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Core.Abstractions;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Infra.Data.Context;
using BGD.CLINICAL.Infra.Data.Repositories;
using BGD.CLINICAL.Infra.Data.Repositories.Core;
using BGD.CLINICAL.Infra.Data.Repositories.Identity;
using BGD.CLINICAL.Infra.Data.Services.Audits;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BGD.CLINICAL.Infra.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddInfraData(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AppDbContext>());
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUsersRepository, UsersRepository>();
        services.AddScoped<IUnitsRepository, UnitsRepository>();
        services.AddScoped<IAuditLogsService, AuditLogsService>();

        return services;
    }
}
