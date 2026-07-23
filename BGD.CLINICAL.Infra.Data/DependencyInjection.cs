using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Core.Abstractions;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Application.Patients.Abstractions;
using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Application.Applications.Abstractions;
using BGD.CLINICAL.Application.Modules.Abstractions;
using BGD.CLINICAL.Application.Notifications.Abstractions;
using BGD.CLINICAL.Application.Packages.Abstractions;
using BGD.CLINICAL.Application.Schedules.Abstractions;
using BGD.CLINICAL.Infra.Data.Context;
using BGD.CLINICAL.Infra.Data.Repositories;
using BGD.CLINICAL.Infra.Data.Repositories.Core;
using BGD.CLINICAL.Infra.Data.Repositories.Identity;
using BGD.CLINICAL.Infra.Data.Repositories.Inventory;
using BGD.CLINICAL.Infra.Data.Repositories.Patients;
using BGD.CLINICAL.Infra.Data.Repositories.Applications;
using BGD.CLINICAL.Infra.Data.Repositories.Modules;
using BGD.CLINICAL.Infra.Data.Repositories.Packages;
using BGD.CLINICAL.Infra.Data.Services.Permissions;
using BGD.CLINICAL.Infra.Data.Repositories.Notifications;
using BGD.CLINICAL.Infra.Data.Repositories.Schedules;
using BGD.CLINICAL.Infra.Data.Services.Audits;
using BGD.CLINICAL.Infra.Data.Services.Inventory;
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
            options.UseSqlServer(connectionString, sql =>
                sql.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null)));

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
        services.AddScoped<IProductLotsRepository, ProductLotsRepository>();
        services.AddScoped<IMeasurementUnitsRepository, MeasurementUnitsRepository>();
        services.AddScoped<IProductsRepository, ProductsRepository>();
        services.AddScoped<ISuppliersRepository, SuppliersRepository>();
        services.AddScoped<ISupplierOrdersRepository, SupplierOrdersRepository>();
        services.AddScoped<ISupplierOrderAttachmentsRepository, SupplierOrderAttachmentsRepository>();
        services.AddScoped<IStockMovementsRepository, StockMovementsRepository>();
        services.AddScoped<IStockBalancesRepository, StockBalancesRepository>();
        services.AddScoped<IAuditLogsService, AuditLogsService>();
        services.AddScoped<IPatientApplicationsRepository, PatientApplicationsRepository>();
        services.AddScoped<IProceduresRepository, ProceduresRepository>();
        services.AddScoped<IPackagesRepository, PackagesRepository>();
        services.AddScoped<IPatientPurchasesRepository, PatientPurchasesRepository>();
        services.AddScoped<ISymptomsRepository, SymptomsRepository>();
        services.AddMemoryCache();
        services.AddScoped<IPermissionCatalogRepository, PermissionCatalogRepository>();
        services.AddScoped<IUserPermissionAssignmentsRepository, UserPermissionAssignmentsRepository>();
        services.AddScoped<IUserPermissionsRepository, UserPermissionsRepository>();
        services.AddScoped<ICompanyModuleLicensesProvisioner, CompanyModuleLicensesProvisioner>();
        services.AddScoped<ICompanyDefaultMeasurementUnitsProvisioner, CompanyDefaultMeasurementUnitsProvisioner>();
        services.AddScoped<ICompanyDefaultProductTypesProvisioner, CompanyDefaultProductTypesProvisioner>();
        services.AddScoped<ISystemModulesRepository, SystemModulesRepository>();
        services.AddScoped<IPermissionChecker, CachedPermissionChecker>();
        services.AddScoped<IPermissionCacheInvalidator, CachedPermissionChecker>();
        services.AddScoped<IModuleLicensesRepository, ModuleLicensesRepository>();
        services.AddScoped<IAppointmentsRepository, AppointmentsRepository>();
        services.AddScoped<IUnitOperatingHoursRepository, UnitOperatingHoursRepository>();
        services.AddScoped<IEmailOutboxRepository, EmailOutboxRepository>();

        return services;
    }
}
