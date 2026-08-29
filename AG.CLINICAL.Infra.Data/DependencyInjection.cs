using AG.CLINICAL.Application.Abstractions.Persistence;
using AG.CLINICAL.Application.Core.Abstractions;
using AG.CLINICAL.Application.Identity.Abstractions;
using AG.CLINICAL.Application.Patients.Abstractions;
using AG.CLINICAL.Application.Inventory.Abstractions;
using AG.CLINICAL.Application.Applications.Abstractions;
using AG.CLINICAL.Application.Modules.Abstractions;
using AG.CLINICAL.Application.Notifications.Abstractions;
using AG.CLINICAL.Application.Packages.Abstractions;
using AG.CLINICAL.Application.Schedules.Abstractions;
using AG.CLINICAL.Application.Reports.Abstractions;
using AG.CLINICAL.Application.ClinicalRecords.Abstractions;
using AG.CLINICAL.Infra.Data.Context;
using AG.CLINICAL.Infra.Data.Repositories;
using AG.CLINICAL.Infra.Data.Repositories.Core;
using AG.CLINICAL.Infra.Data.Repositories.Identity;
using AG.CLINICAL.Infra.Data.Repositories.Inventory;
using AG.CLINICAL.Infra.Data.Repositories.Patients;
using AG.CLINICAL.Infra.Data.Repositories.Applications;
using AG.CLINICAL.Infra.Data.Repositories.Modules;
using AG.CLINICAL.Infra.Data.Repositories.Packages;
using AG.CLINICAL.Infra.Data.Services.Permissions;
using AG.CLINICAL.Infra.Data.Repositories.Notifications;
using AG.CLINICAL.Application.Audits.Abstractions;
using AG.CLINICAL.Infra.Data.Repositories.Audits;
using AG.CLINICAL.Infra.Data.Repositories.Schedules;
using AG.CLINICAL.Infra.Data.Repositories.Reports;
using AG.CLINICAL.Infra.Data.Repositories.ClinicalRecords;
using AG.CLINICAL.Infra.Data.Services.Audits;
using AG.CLINICAL.Infra.Data.Services.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AG.CLINICAL.Infra.Data;

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
        services.AddScoped<IAuditLogsQueryRepository, AuditLogsQueryRepository>();
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
        services.AddScoped<IOperationalReportsRepository, OperationalReportsRepository>();
        services.AddScoped<IMedicalRecordsRepository, MedicalRecordsRepository>();
        services.AddScoped<IClinicalEncountersRepository, ClinicalEncountersRepository>();
        services.AddScoped<IClinicalNotesRepository, ClinicalNotesRepository>();
        services.AddScoped<IAnamneseTemplatesRepository, AnamneseTemplatesRepository>();
        services.AddScoped<IAnamneseRecordsRepository, AnamneseRecordsRepository>();
        services.AddScoped<IBodyAssessmentsRepository, BodyAssessmentsRepository>();
        services.AddScoped<IClinicalFilesRepository, ClinicalFilesRepository>();
        services.AddScoped<INutritionRecordsRepository, NutritionRecordsRepository>();

        return services;
    }
}
