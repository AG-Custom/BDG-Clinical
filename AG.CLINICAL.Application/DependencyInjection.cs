using AG.CLINICAL.Application.Abstractions.Identity;
using AG.CLINICAL.Application.Identity.Abstractions;
using AG.CLINICAL.Application.Identity.Authentications;
using AG.CLINICAL.Application.Identity.Registrations;
using AG.CLINICAL.Application.Identity.FirstAccess;
using AG.CLINICAL.Application.Audits;
using AG.CLINICAL.Application.Identity.Users;
using AG.CLINICAL.Application.Core.Units;
using AG.CLINICAL.Application.Core.Companies;
using AG.CLINICAL.Application.Core.Employees;
using AG.CLINICAL.Application.Core.Positions;
using AG.CLINICAL.Application.Patients.Patients;
using AG.CLINICAL.Application.Patients.Symptoms;
using AG.CLINICAL.Application.Inventory.Abstractions;
using AG.CLINICAL.Application.Inventory.ProductTypes;
using AG.CLINICAL.Application.Inventory.Products;
using AG.CLINICAL.Application.Inventory.MeasurementUnits;
using AG.CLINICAL.Application.Inventory.Suppliers;
using AG.CLINICAL.Application.Inventory.SupplierOrders;
using AG.CLINICAL.Application.Inventory.StockBalances;
using AG.CLINICAL.Application.Inventory.StockMovements;
using AG.CLINICAL.Application.Applications.PatientApplications;
using AG.CLINICAL.Application.Applications.Procedures;
using AG.CLINICAL.Application.Packages.Packages;
using AG.CLINICAL.Application.Packages.PatientPurchases;
using AG.CLINICAL.Application.Schedules.Appointments;
using AG.CLINICAL.Application.Schedules.UnitOperatingHours;
using AG.CLINICAL.Application.Modules.EmployeePermissions;
using AG.CLINICAL.Application.Core.PositionPermissions;
using AG.CLINICAL.Application.Modules.Permissions;
using AG.CLINICAL.Application.Notifications.EmailOutbox;
using AG.CLINICAL.Application.Reports;
using Microsoft.Extensions.DependencyInjection;

namespace AG.CLINICAL.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ILoginUsersService, LoginUsersService>();
        services.AddScoped<ISwitchCompanyService, SwitchCompanyService>();
        services.AddScoped<IRegisterCompaniesService, RegisterCompaniesService>();
        services.AddScoped<IGetAuthenticatedUsersService, GetAuthenticatedUsersService>();
        services.AddScoped<IResolveUserDisplayNamesService, ResolveUserDisplayNamesService>();
        services.AddScoped<IGetEntityAuditSummaryService, GetEntityAuditSummaryService>();
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

        services.AddScoped<ICreatePatientsService, CreatePatientsService>();
        services.AddScoped<IListPatientsService, ListPatientsService>();
        services.AddScoped<IGetPatientsService, GetPatientsService>();
        services.AddScoped<IUpdatePatientsService, UpdatePatientsService>();
        services.AddScoped<IDeactivatePatientsService, DeactivatePatientsService>();
        services.AddScoped<IReactivatePatientsService, ReactivatePatientsService>();

        services.AddScoped<ICreateSymptomsService, CreateSymptomsService>();
        services.AddScoped<IListSymptomsService, ListSymptomsService>();
        services.AddScoped<IGetSymptomsService, GetSymptomsService>();
        services.AddScoped<IUpdateSymptomsService, UpdateSymptomsService>();
        services.AddScoped<IDeactivateSymptomsService, DeactivateSymptomsService>();
        services.AddScoped<IReactivateSymptomsService, ReactivateSymptomsService>();

        services.AddScoped<ICreateProductTypesService, CreateProductTypesService>();
        services.AddScoped<IListProductTypesService, ListProductTypesService>();
        services.AddScoped<IGetProductTypesService, GetProductTypesService>();
        services.AddScoped<IUpdateProductTypesService, UpdateProductTypesService>();
        services.AddScoped<IDeactivateProductTypesService, DeactivateProductTypesService>();
        services.AddScoped<IReactivateProductTypesService, ReactivateProductTypesService>();

        services.AddScoped<ICreateMeasurementUnitsService, CreateMeasurementUnitsService>();
        services.AddScoped<IListMeasurementUnitsService, ListMeasurementUnitsService>();
        services.AddScoped<IGetMeasurementUnitsService, GetMeasurementUnitsService>();
        services.AddScoped<IUpdateMeasurementUnitsService, UpdateMeasurementUnitsService>();
        services.AddScoped<IDeactivateMeasurementUnitsService, DeactivateMeasurementUnitsService>();
        services.AddScoped<IReactivateMeasurementUnitsService, ReactivateMeasurementUnitsService>();

        services.AddScoped<ICreateProductsService, CreateProductsService>();
        services.AddScoped<IListProductsService, ListProductsService>();
        services.AddScoped<IGetProductsService, GetProductsService>();
        services.AddScoped<IUpdateProductsService, UpdateProductsService>();
        services.AddScoped<IDeactivateProductsService, DeactivateProductsService>();
        services.AddScoped<IReactivateProductsService, ReactivateProductsService>();

        services.AddScoped<ICreateSuppliersService, CreateSuppliersService>();
        services.AddScoped<IListSuppliersService, ListSuppliersService>();
        services.AddScoped<IGetSuppliersService, GetSuppliersService>();
        services.AddScoped<IUpdateSuppliersService, UpdateSuppliersService>();
        services.AddScoped<IDeactivateSuppliersService, DeactivateSuppliersService>();
        services.AddScoped<IReactivateSuppliersService, ReactivateSuppliersService>();

        services.AddScoped<ICreateSupplierOrdersService, CreateSupplierOrdersService>();
        services.AddScoped<IListSupplierOrdersService, ListSupplierOrdersService>();
        services.AddScoped<IGetSupplierOrdersService, GetSupplierOrdersService>();
        services.AddScoped<IUpdateSupplierOrdersService, UpdateSupplierOrdersService>();
        services.AddScoped<ICancelSupplierOrdersService, CancelSupplierOrdersService>();
        services.AddScoped<IReceiveSupplierOrdersService, ReceiveSupplierOrdersService>();
        services.AddScoped<IUploadSupplierOrderAttachmentService, UploadSupplierOrderAttachmentService>();
        services.AddScoped<IListSupplierOrderAttachmentsService, ListSupplierOrderAttachmentsService>();
        services.AddScoped<IDeleteSupplierOrderAttachmentService, DeleteSupplierOrderAttachmentService>();

        services.AddScoped<IListStockBalancesService, ListStockBalancesService>();
        services.AddScoped<IListStockLotBalancesService, ListStockLotBalancesService>();
        services.AddScoped<IListStockMovementsService, ListStockMovementsService>();
        services.AddScoped<ICreateStockAdjustmentsService, CreateStockAdjustmentsService>();
        services.AddScoped<ICreateStockLossesService, CreateStockLossesService>();
        services.AddScoped<ICreateStockTransferService, CreateStockTransferService>();
        services.AddScoped<IMedicationLotStockService, MedicationLotStockService>();

        services.AddScoped<ICreatePatientApplicationsService, CreatePatientApplicationsService>();
        services.AddScoped<IListPatientApplicationsService, ListPatientApplicationsService>();
        services.AddScoped<IGetPatientApplicationsService, GetPatientApplicationsService>();
        services.AddScoped<IUpdatePatientApplicationsService, UpdatePatientApplicationsService>();
        services.AddScoped<ICancelPatientApplicationsService, CancelPatientApplicationsService>();

        services.AddScoped<ICreateProceduresService, CreateProceduresService>();
        services.AddScoped<IListProceduresService, ListProceduresService>();
        services.AddScoped<IGetProceduresService, GetProceduresService>();
        services.AddScoped<IUpdateProceduresService, UpdateProceduresService>();
        services.AddScoped<IDeactivateProceduresService, DeactivateProceduresService>();
        services.AddScoped<IReactivateProceduresService, ReactivateProceduresService>();

        services.AddScoped<ICreatePackagesService, CreatePackagesService>();
        services.AddScoped<IListPackagesService, ListPackagesService>();
        services.AddScoped<IGetPackagesService, GetPackagesService>();
        services.AddScoped<IUpdatePackagesService, UpdatePackagesService>();
        services.AddScoped<IDeactivatePackagesService, DeactivatePackagesService>();
        services.AddScoped<IReactivatePackagesService, ReactivatePackagesService>();

        services.AddScoped<ICreatePatientPurchasesService, CreatePatientPurchasesService>();
        services.AddScoped<IListAllPatientPurchasesService, ListAllPatientPurchasesService>();
        services.AddScoped<IListPatientPurchasesService, ListPatientPurchasesService>();
        services.AddScoped<IListActivePatientPurchasesService, ListActivePatientPurchasesService>();
        services.AddScoped<IGetPatientPurchasesService, GetPatientPurchasesService>();
        services.AddScoped<IGetPatientPurchaseBalanceService, GetPatientPurchaseBalanceService>();
        services.AddScoped<IGetPatientPurchaseHistoryService, GetPatientPurchaseHistoryService>();
        services.AddScoped<IUpdatePatientPurchaseBalancesService, UpdatePatientPurchaseBalancesService>();
        services.AddScoped<ICancelPatientPurchasesService, CancelPatientPurchasesService>();

        services.AddScoped<ICreateAppointmentsService, CreateAppointmentsService>();
        services.AddScoped<IEmailOutboxEnqueueService, EmailOutboxEnqueueService>();
        services.AddScoped<IProcessEmailOutboxService, ProcessEmailOutboxService>();
        services.AddScoped<IListAppointmentsService, ListAppointmentsService>();
        services.AddScoped<IGetAppointmentsService, GetAppointmentsService>();
        services.AddScoped<IUpdateAppointmentsService, UpdateAppointmentsService>();
        services.AddScoped<IConfirmAppointmentsService, ConfirmAppointmentsService>();
        services.AddScoped<ICompleteAppointmentsService, CompleteAppointmentsService>();
        services.AddScoped<ICancelAppointmentsService, CancelAppointmentsService>();
        services.AddScoped<IMarkNoShowAppointmentsService, MarkNoShowAppointmentsService>();

        services.AddScoped<IListUnitOperatingHoursService, ListUnitOperatingHoursService>();
        services.AddScoped<ICreateUnitOperatingHoursService, CreateUnitOperatingHoursService>();
        services.AddScoped<IUpdateUnitOperatingHoursService, UpdateUnitOperatingHoursService>();
        services.AddScoped<ISetUnitOperatingHourActiveStatusService, SetUnitOperatingHourActiveStatusService>();

        services.AddScoped<IGetPermissionMapService, GetPermissionMapService>();
        services.AddScoped<IGetPositionPermissionsService, GetPositionPermissionsService>();
        services.AddScoped<IUpdatePositionPermissionsService, UpdatePositionPermissionsService>();
        services.AddScoped<IGetEmployeePermissionsService, GetEmployeePermissionsService>();
        services.AddScoped<IUpdateEmployeePermissionsService, UpdateEmployeePermissionsService>();
        services.AddScoped<IUsersPermissionMutationService, UsersPermissionMutationService>();
        services.AddScoped<IGetOperationalReportService, GetOperationalReportService>();

        return services;
    }

    public static IServiceCollection AddApplicationJobs(this IServiceCollection services)
    {
        services.AddScoped<IProcessEmailOutboxService, ProcessEmailOutboxService>();

        return services;
    }
}
