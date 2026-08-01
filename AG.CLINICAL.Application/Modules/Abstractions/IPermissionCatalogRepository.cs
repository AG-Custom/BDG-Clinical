using AG.CLINICAL.Application.Modules.Permissions;
using AG.CLINICAL.Domain.Entities;

namespace AG.CLINICAL.Application.Modules.Abstractions;

public interface IPermissionCatalogRepository
{
    Task<IReadOnlyList<PermissaoSistema>> ListAllAsync(CancellationToken cancellationToken = default);

    Task<bool> SyncCatalogAsync(
        IReadOnlyList<PermissionDefinition> definitions,
        CancellationToken cancellationToken = default);
}
