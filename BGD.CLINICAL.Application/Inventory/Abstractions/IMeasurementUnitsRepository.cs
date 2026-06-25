using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;

namespace BGD.CLINICAL.Application.Inventory.Abstractions;

public interface IMeasurementUnitsRepository
{
    Task<IReadOnlyList<UnidadeMedida>> ListByEmpresaIdAsync(
        Guid empresaId,
        bool includeInactive,
        TipoUnidadeMedida? tipo,
        string? search,
        int? limit,
        CancellationToken cancellationToken = default);

    Task<UnidadeMedida?> GetByIdAndEmpresaIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByNomeAsync(
        Guid empresaId,
        string nome,
        Guid? excludeId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsBySiglaAsync(
        Guid empresaId,
        string sigla,
        Guid? excludeId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsActiveByIdAndEmpresaIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default);

    Task<bool> HasActiveProductsAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default);

    Task AddAsync(UnidadeMedida unidadeMedida, CancellationToken cancellationToken = default);

    void Update(UnidadeMedida unidadeMedida);
}
