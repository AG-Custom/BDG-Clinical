using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;

namespace BGD.CLINICAL.Application.Inventory.Abstractions;

public interface ISuppliersRepository
{
    Task<IReadOnlyList<Fornecedor>> ListByEmpresaIdAsync(
        Guid empresaId,
        bool includeInactive,
        string? search,
        int? limit,
        CancellationToken cancellationToken = default);

    Task<Fornecedor?> GetByIdAndEmpresaIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByNomeAsync(
        Guid empresaId,
        string nome,
        Guid? excludeId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByCnpjAsync(
        Guid empresaId,
        string cnpj,
        Guid? excludeId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsActiveByIdAndEmpresaIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default);

    Task<bool> HasOpenPedidosAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default);

    Task AddAsync(Fornecedor fornecedor, CancellationToken cancellationToken = default);

    void Update(Fornecedor fornecedor);
}
