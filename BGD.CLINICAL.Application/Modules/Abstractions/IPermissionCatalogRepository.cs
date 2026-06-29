using BGD.CLINICAL.Application.Modules.Permissions;
using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Application.Modules.Abstractions;

public interface IPermissionCatalogRepository
{
    Task<IReadOnlyList<PermissaoSistema>> ListAllAsync(CancellationToken cancellationToken = default);

    Task<bool> SyncCatalogAsync(
        IReadOnlyList<PermissionDefinition> definitions,
        CancellationToken cancellationToken = default);
}
