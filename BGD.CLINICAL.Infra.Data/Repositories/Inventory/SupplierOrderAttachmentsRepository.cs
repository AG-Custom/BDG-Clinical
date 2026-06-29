using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace BGD.CLINICAL.Infra.Data.Repositories.Inventory;

public sealed class SupplierOrderAttachmentsRepository : ISupplierOrderAttachmentsRepository
{
    private readonly AppDbContext _context;

    public SupplierOrderAttachmentsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AnexoPedidoFornecedor>> ListByPedidoIdAndEmpresaIdAsync(
        Guid pedidoId,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return await _context.AnexosPedidoFornecedor
            .AsNoTracking()
            .Where(anexo => anexo.PedidoFornecedorId == pedidoId && anexo.EmpresaId == empresaId)
            .OrderByDescending(anexo => anexo.CriadoEm)
            .ToListAsync(cancellationToken);
    }

    public Task<AnexoPedidoFornecedor?> GetByIdPedidoIdAndEmpresaIdAsync(
        Guid id,
        Guid pedidoId,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return _context.AnexosPedidoFornecedor
            .FirstOrDefaultAsync(
                anexo => anexo.Id == id
                    && anexo.PedidoFornecedorId == pedidoId
                    && anexo.EmpresaId == empresaId,
                cancellationToken);
    }

    public async Task AddAsync(AnexoPedidoFornecedor anexo, CancellationToken cancellationToken = default)
    {
        await _context.AnexosPedidoFornecedor.AddAsync(anexo, cancellationToken);
    }

    public void Remove(AnexoPedidoFornecedor anexo)
    {
        _context.AnexosPedidoFornecedor.Remove(anexo);
    }
}
