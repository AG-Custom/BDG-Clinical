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
using BGD.CLINICAL.Application.Patients.Patients;
using BGD.CLINICAL.Application.Patients.Symptoms;
using BGD.CLINICAL.Application.Inventory.ProductTypes;
using BGD.CLINICAL.Application.Inventory.Products;
using BGD.CLINICAL.Application.Inventory.MeasurementUnits;
using BGD.CLINICAL.Application.Inventory.Suppliers;
using BGD.CLINICAL.Application.Inventory.SupplierOrders;
using BGD.CLINICAL.Application.Inventory.StockBalances;
using BGD.CLINICAL.Application.Inventory.StockMovements;
using BGD.CLINICAL.Application.Applications.PatientApplications;
using BGD.CLINICAL.Application.Applications.Procedures;
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

        services.AddScoped<IListStockBalancesService, ListStockBalancesService>();
        services.AddScoped<IListStockMovementsService, ListStockMovementsService>();
        services.AddScoped<ICreateStockAdjustmentsService, CreateStockAdjustmentsService>();
        services.AddScoped<ICreateStockLossesService, CreateStockLossesService>();

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

        return services;
    }
}
