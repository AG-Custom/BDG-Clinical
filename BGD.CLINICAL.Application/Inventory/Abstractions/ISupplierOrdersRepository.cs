using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;

namespace BGD.CLINICAL.Application.Inventory.Abstractions;

public interface ISupplierOrdersRepository
{
    Task<IReadOnlyList<PedidoFornecedor>> ListByEmpresaIdAsync(
        Guid empresaId,
        StatusPedidoFornecedor? status,
        Guid? fornecedorId,
        Guid? unidadeId,
        CancellationToken cancellationToken = default);

    Task<PedidoFornecedor?> GetByIdAndEmpresaIdWithItensAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default);

    Task<PedidoFornecedor?> GetByIdAndEmpresaIdWithItensAsNoTrackingAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default);

    Task<PedidoFornecedor?> GetByIdAndEmpresaIdForUpdateAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default);

    Task AddAsync(PedidoFornecedor pedido, CancellationToken cancellationToken = default);

    void Update(PedidoFornecedor pedido);

    Task ReplaceItensAsync(
        PedidoFornecedor pedido,
        IReadOnlyList<ItemPedidoFornecedor> newItens,
        CancellationToken cancellationToken = default);
}
