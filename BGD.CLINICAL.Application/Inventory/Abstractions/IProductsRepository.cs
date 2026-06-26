using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Application.Inventory.Abstractions;

public interface IProductsRepository
{
    Task<IReadOnlyList<Produto>> ListByEmpresaIdAsync(
        Guid empresaId,
        Guid? tipoProdutoId,
        bool includeInactive,
        CancellationToken cancellationToken = default);

    Task<Produto?> GetByIdAndEmpresaIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByNomeAsync(
        Guid empresaId,
        string nome,
        Guid? excludeId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsActiveTipoProdutoInEmpresaAsync(
        Guid tipoProdutoId,
        Guid empresaId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsActiveUnidadeMedidaInEmpresaAsync(
        Guid unidadeMedidaId,
        Guid empresaId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsActiveByIdAndEmpresaIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsBySkuAsync(
        Guid empresaId,
        string sku,
        Guid? excludeId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByCodigoInternoAsync(
        Guid empresaId,
        string codigoInterno,
        Guid? excludeId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Produto>> GetActiveByIdsAndEmpresaIdAsync(
        Guid empresaId,
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default);

    Task AddAsync(Produto produto, CancellationToken cancellationToken = default);

    void Update(Produto produto);
}
