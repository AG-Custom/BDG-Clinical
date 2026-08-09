using AG.CLINICAL.Application.Abstractions.Persistence;
using AG.CLINICAL.Application.Modules.Abstractions;
using AG.CLINICAL.Application.Modules.Permissions;

namespace AG.CLINICAL.Infra.Data.Services.Permissions;

public static class PermissionSeeder
{
    public static async Task SeedAsync(
        IPermissionCatalogRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken = default)
    {
        var hasChanges = await repository.SyncCatalogAsync(PermissionCatalog.All, cancellationToken);

        if (hasChanges)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
