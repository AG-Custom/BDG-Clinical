using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Application.Inventory.Abstractions;

public interface ISupplierOrderAttachmentsRepository
{
    Task<IReadOnlyList<AnexoPedidoFornecedor>> ListByPedidoIdAndEmpresaIdAsync(
        Guid pedidoId,
        Guid empresaId,
        CancellationToken cancellationToken = default);

    Task<AnexoPedidoFornecedor?> GetByIdPedidoIdAndEmpresaIdAsync(
        Guid id,
        Guid pedidoId,
        Guid empresaId,
        CancellationToken cancellationToken = default);

    Task AddAsync(AnexoPedidoFornecedor anexo, CancellationToken cancellationToken = default);

    void Remove(AnexoPedidoFornecedor anexo);
}
