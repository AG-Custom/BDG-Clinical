using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Application.Inventory.Abstractions;

public interface IProductTypesRepository
{
    Task<IReadOnlyList<TipoProduto>> ListByEmpresaIdAsync(
        Guid empresaId,
        bool includeInactive,
        CancellationToken cancellationToken = default);

    Task<TipoProduto?> GetByIdAndEmpresaIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByNomeAsync(
        Guid empresaId,
        string nome,
        Guid? excludeId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByCodigoAsync(
        Guid empresaId,
        string codigo,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TipoProduto>> ListByExactNomeWithoutCodigoAsync(
        Guid empresaId,
        string nome,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsActiveByIdAndEmpresaIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default);

    Task AddAsync(TipoProduto tipoProduto, CancellationToken cancellationToken = default);

    void Update(TipoProduto tipoProduto);
}
