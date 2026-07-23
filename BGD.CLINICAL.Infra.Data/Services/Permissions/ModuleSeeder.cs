using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Application.Modules.Abstractions;

namespace BGD.CLINICAL.Infra.Data.Services.Permissions;

public static class ModuleSeeder
{
    public static async Task SeedAsync(
        ISystemModulesRepository repository,
        ICompanyModuleLicensesProvisioner moduleLicensesProvisioner,
        ICompanyDefaultMeasurementUnitsProvisioner measurementUnitsProvisioner,
        ICompanyDefaultProductTypesProvisioner productTypesProvisioner,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken = default)
    {
        var hasChanges = await repository.SyncCatalogAsync(cancellationToken);

        if (hasChanges)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        await moduleLicensesProvisioner.BackfillAllCompaniesAsync(cancellationToken);
        await measurementUnitsProvisioner.BackfillAllCompaniesAsync(cancellationToken);
        await productTypesProvisioner.BackfillAllCompaniesAsync(cancellationToken);
    }
}
