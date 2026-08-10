using AG.CLINICAL.Application.Abstractions.Persistence;
using AG.CLINICAL.Application.Inventory.Abstractions;
using AG.CLINICAL.Application.Modules.Abstractions;

namespace AG.CLINICAL.Infra.Data.Services.Permissions;

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
