using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Modules.Abstractions;
using BGD.CLINICAL.Application.Modules.Permissions;

namespace BGD.CLINICAL.Infra.Data.Services.Permissions;

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
