using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Application.Modules.Abstractions;

public interface ISystemModulesRepository
{
    Task<bool> SyncCatalogAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ModuloSistema>> ListActiveAsync(CancellationToken cancellationToken = default);
}
