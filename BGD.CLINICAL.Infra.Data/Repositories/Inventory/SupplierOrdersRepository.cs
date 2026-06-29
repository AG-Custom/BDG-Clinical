using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using BGD.CLINICAL.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace BGD.CLINICAL.Infra.Data.Repositories.Inventory;

public sealed class SupplierOrdersRepository : ISupplierOrdersRepository
{
    private readonly AppDbContext _context;

    public SupplierOrdersRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PedidoFornecedor>> ListByEmpresaIdAsync(
        Guid empresaId,
        StatusPedidoFornecedor? status,
        Guid? fornecedorId,
        Guid? unidadeId,
        CancellationToken cancellationToken = default)
    {
        var query = _context.PedidosFornecedor
            .AsNoTracking()
            .Include(pedido => pedido.Fornecedor)
            .Include(pedido => pedido.Unidade)
            .Include(pedido => pedido.Itens)
                .ThenInclude(item => item.Produto)
            .Include(pedido => pedido.Anexos)
            .Where(pedido => pedido.EmpresaId == empresaId);

        if (status.HasValue)
        {
            query = query.Where(pedido => pedido.Status == status.Value);
        }

        if (fornecedorId.HasValue)
        {
            query = query.Where(pedido => pedido.FornecedorId == fornecedorId.Value);
        }

        if (unidadeId.HasValue)
        {
            query = query.Where(pedido => pedido.UnidadeId == unidadeId.Value);
        }

        return await query
            .OrderByDescending(pedido => pedido.DataPedido)
            .ThenBy(pedido => pedido.Fornecedor.Nome)
            .ToListAsync(cancellationToken);
    }

    public Task<PedidoFornecedor?> GetByIdAndEmpresaIdWithItensAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return _context.PedidosFornecedor
            .Include(pedido => pedido.Fornecedor)
            .Include(pedido => pedido.Unidade)
            .Include(pedido => pedido.Itens)
                .ThenInclude(item => item.Produto)
            .Include(pedido => pedido.Anexos)
            .FirstOrDefaultAsync(
                pedido => pedido.Id == id && pedido.EmpresaId == empresaId,
                cancellationToken);
    }

    public Task<PedidoFornecedor?> GetByIdAndEmpresaIdWithItensAsNoTrackingAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return _context.PedidosFornecedor
            .AsNoTracking()
            .Include(pedido => pedido.Fornecedor)
            .Include(pedido => pedido.Unidade)
            .Include(pedido => pedido.Itens)
                .ThenInclude(item => item.Produto)
            .Include(pedido => pedido.Anexos)
            .FirstOrDefaultAsync(
                pedido => pedido.Id == id && pedido.EmpresaId == empresaId,
                cancellationToken);
    }

    public Task<PedidoFornecedor?> GetByIdAndEmpresaIdForUpdateAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return _context.PedidosFornecedor
            .FirstOrDefaultAsync(
                pedido => pedido.Id == id && pedido.EmpresaId == empresaId,
                cancellationToken);
    }

    public async Task AddAsync(PedidoFornecedor pedido, CancellationToken cancellationToken = default)
    {
        await _context.PedidosFornecedor.AddAsync(pedido, cancellationToken);
    }

    public void Update(PedidoFornecedor pedido)
    {
        var entry = _context.Entry(pedido);

        if (entry.State == EntityState.Detached)
        {
            _context.PedidosFornecedor.Update(pedido);
        }
    }

    public async Task ReplaceItensAsync(
        PedidoFornecedor pedido,
        IReadOnlyList<ItemPedidoFornecedor> newItens,
        CancellationToken cancellationToken = default)
    {
        var existingItens = await _context.ItensPedidoFornecedor
            .Where(item => item.PedidoFornecedorId == pedido.Id)
            .ToListAsync(cancellationToken);

        if (existingItens.Count > 0)
        {
            _context.ItensPedidoFornecedor.RemoveRange(existingItens);
        }

        await _context.ItensPedidoFornecedor.AddRangeAsync(newItens, cancellationToken);
    }
}

