using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace BGD.CLINICAL.Infra.Data.Repositories.Inventory;

public sealed class ProductsRepository : IProductsRepository
{
    private readonly AppDbContext _context;

    public ProductsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Produto>> ListByEmpresaIdAsync(
        Guid empresaId,
        Guid? tipoProdutoId,
        bool includeInactive,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Produtos
            .AsNoTracking()
            .Include(produto => produto.TipoProduto)
            .Include(produto => produto.UnidadeMedida)
            .Where(produto => produto.EmpresaId == empresaId);

        if (tipoProdutoId.HasValue)
        {
            query = query.Where(produto => produto.TipoProdutoId == tipoProdutoId.Value);
        }

        if (!includeInactive)
        {
            query = query.Where(produto => produto.Ativo);
        }

        return await query
            .OrderBy(produto => produto.Nome)
            .ToListAsync(cancellationToken);
    }

    public Task<Produto?> GetByIdAndEmpresaIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return _context.Produtos
            .Include(produto => produto.TipoProduto)
            .Include(produto => produto.UnidadeMedida)
            .FirstOrDefaultAsync(
                produto => produto.Id == id && produto.EmpresaId == empresaId,
                cancellationToken);
    }

    public Task<bool> ExistsByNomeAsync(
        Guid empresaId,
        string nome,
        Guid? excludeId,
        CancellationToken cancellationToken = default)
    {
        var normalizedNome = nome.Trim().ToUpperInvariant();

        return _context.Produtos.AnyAsync(
            produto => produto.EmpresaId == empresaId
                && produto.Nome.ToUpper() == normalizedNome
                && (!excludeId.HasValue || produto.Id != excludeId.Value),
            cancellationToken);
    }

    public Task<bool> ExistsActiveTipoProdutoInEmpresaAsync(
        Guid tipoProdutoId,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return _context.TiposProduto.AnyAsync(
            tipo => tipo.Id == tipoProdutoId && tipo.EmpresaId == empresaId && tipo.Ativo,
            cancellationToken);
    }

    public Task<bool> ExistsActiveUnidadeMedidaInEmpresaAsync(
        Guid unidadeMedidaId,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return _context.UnidadesMedida.AnyAsync(
            unidade => unidade.Id == unidadeMedidaId && unidade.EmpresaId == empresaId && unidade.Ativo,
            cancellationToken);
    }

    public Task<bool> ExistsActiveByIdAndEmpresaIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return _context.Produtos.AnyAsync(
            produto => produto.Id == id && produto.EmpresaId == empresaId && produto.Ativo,
            cancellationToken);
    }

    public Task<bool> ExistsBySkuAsync(
        Guid empresaId,
        string sku,
        Guid? excludeId,
        CancellationToken cancellationToken = default)
    {
        var normalizedSku = sku.Trim().ToUpperInvariant();

        return _context.Produtos.AnyAsync(
            produto => produto.EmpresaId == empresaId
                && produto.Sku != null
                && produto.Sku.ToUpper() == normalizedSku
                && (!excludeId.HasValue || produto.Id != excludeId.Value),
            cancellationToken);
    }

    public Task<bool> ExistsByCodigoInternoAsync(
        Guid empresaId,
        string codigoInterno,
        Guid? excludeId,
        CancellationToken cancellationToken = default)
    {
        var normalizedCodigo = codigoInterno.Trim().ToUpperInvariant();

        return _context.Produtos.AnyAsync(
            produto => produto.EmpresaId == empresaId
                && produto.CodigoInterno != null
                && produto.CodigoInterno.ToUpper() == normalizedCodigo
                && (!excludeId.HasValue || produto.Id != excludeId.Value),
            cancellationToken);
    }

    public async Task<IReadOnlyList<Produto>> GetActiveByIdsAndEmpresaIdAsync(
        Guid empresaId,
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
        {
            return [];
        }

        return await _context.Produtos
            .AsNoTracking()
            .Where(produto => produto.EmpresaId == empresaId
                && produto.Ativo
                && ids.Contains(produto.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Produto produto, CancellationToken cancellationToken = default)
    {
        await _context.Produtos.AddAsync(produto, cancellationToken);
    }

    public void Update(Produto produto)
    {
        var entry = _context.Entry(produto);

        if (entry.State == EntityState.Detached)
        {
            _context.Produtos.Update(produto);
        }
    }
}
