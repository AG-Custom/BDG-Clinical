using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Core.Abstractions;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Application.Patients.Abstractions;
using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Application.Applications.Abstractions;
using BGD.CLINICAL.Infra.Data.Context;
using BGD.CLINICAL.Infra.Data.Repositories;
using BGD.CLINICAL.Infra.Data.Repositories.Core;
using BGD.CLINICAL.Infra.Data.Repositories.Identity;
using BGD.CLINICAL.Infra.Data.Repositories.Inventory;
using BGD.CLINICAL.Infra.Data.Repositories.Patients;
using BGD.CLINICAL.Infra.Data.Repositories.Applications;
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
        services.AddScoped<IFirstAccessInvitationsRepository, FirstAccessInvitationsRepository>();
        services.AddScoped<IUnitsRepository, UnitsRepository>();
        services.AddScoped<IPositionsRepository, PositionsRepository>();
        services.AddScoped<ICompaniesRepository, CompaniesRepository>();
        services.AddScoped<IEmployeesRepository, EmployeesRepository>();
        services.AddScoped<IPatientsRepository, PatientsRepository>();
        services.AddScoped<IProductTypesRepository, ProductTypesRepository>();
        services.AddScoped<IMeasurementUnitsRepository, MeasurementUnitsRepository>();
        services.AddScoped<IProductsRepository, ProductsRepository>();
        services.AddScoped<ISuppliersRepository, SuppliersRepository>();
        services.AddScoped<ISupplierOrdersRepository, SupplierOrdersRepository>();
        services.AddScoped<IStockMovementsRepository, StockMovementsRepository>();
        services.AddScoped<IStockBalancesRepository, StockBalancesRepository>();
        services.AddScoped<IAuditLogsService, AuditLogsService>();
        services.AddScoped<IPatientApplicationsRepository, PatientApplicationsRepository>();
        services.AddScoped<IProceduresRepository, ProceduresRepository>();
        services.AddScoped<ISymptomsRepository, SymptomsRepository>();

        return services;
    }
}
