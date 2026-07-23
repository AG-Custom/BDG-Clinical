using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace BGD.CLINICAL.Infra.Data.Repositories.Inventory;

public sealed class ProductLotsRepository : IProductLotsRepository
{
    private readonly AppDbContext _context;

    public ProductLotsRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<LoteProduto?> GetByCodigoAsync(
        Guid empresaId,
        Guid unidadeId,
        Guid produtoId,
        string codigo,
        CancellationToken cancellationToken = default)
    {
        var normalized = codigo.Trim().ToUpperInvariant();

        return _context.LotesProduto.FirstOrDefaultAsync(
            lote => lote.EmpresaId == empresaId
                && lote.UnidadeId == unidadeId
                && lote.ProdutoId == produtoId
                && lote.Codigo.ToUpper() == normalized,
            cancellationToken);
    }

    public Task<LoteProduto?> GetByIdAndEmpresaIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return _context.LotesProduto.FirstOrDefaultAsync(
            lote => lote.Id == id && lote.EmpresaId == empresaId,
            cancellationToken);
    }

    public async Task AddAsync(LoteProduto lote, CancellationToken cancellationToken = default)
    {
        await _context.LotesProduto.AddAsync(lote, cancellationToken);
    }
}
