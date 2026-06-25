using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace BGD.CLINICAL.Infra.Data.Repositories.Inventory;

public sealed class ProductTypesRepository : IProductTypesRepository
{
    private readonly AppDbContext _context;

    public ProductTypesRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<TipoProduto>> ListByEmpresaIdAsync(
        Guid empresaId,
        bool includeInactive,
        CancellationToken cancellationToken = default)
    {
        var query = _context.TiposProduto
            .AsNoTracking()
            .Where(tipo => tipo.EmpresaId == empresaId);

        if (!includeInactive)
        {
            query = query.Where(tipo => tipo.Ativo);
        }

        return await query
            .OrderBy(tipo => tipo.Nome)
            .ToListAsync(cancellationToken);
    }

    public Task<TipoProduto?> GetByIdAndEmpresaIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return _context.TiposProduto
            .FirstOrDefaultAsync(
                tipo => tipo.Id == id && tipo.EmpresaId == empresaId,
                cancellationToken);
    }

    public Task<bool> ExistsByNomeAsync(
        Guid empresaId,
        string nome,
        Guid? excludeId,
        CancellationToken cancellationToken = default)
    {
        var normalizedNome = nome.Trim().ToUpperInvariant();

        return _context.TiposProduto.AnyAsync(
            tipo => tipo.EmpresaId == empresaId
                && tipo.Nome.ToUpper() == normalizedNome
                && (!excludeId.HasValue || tipo.Id != excludeId.Value),
            cancellationToken);
    }

    public Task<bool> ExistsActiveByIdAndEmpresaIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return _context.TiposProduto.AnyAsync(
            tipo => tipo.Id == id && tipo.EmpresaId == empresaId && tipo.Ativo,
            cancellationToken);
    }

    public async Task AddAsync(TipoProduto tipoProduto, CancellationToken cancellationToken = default)
    {
        await _context.TiposProduto.AddAsync(tipoProduto, cancellationToken);
    }

    public void Update(TipoProduto tipoProduto)
    {
        var entry = _context.Entry(tipoProduto);

        if (entry.State == EntityState.Detached)
        {
            _context.TiposProduto.Update(tipoProduto);
        }
    }
}
