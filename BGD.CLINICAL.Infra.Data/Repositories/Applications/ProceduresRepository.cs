using BGD.CLINICAL.Application.Applications.Abstractions;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace BGD.CLINICAL.Infra.Data.Repositories.Applications;

public sealed class ProceduresRepository : IProceduresRepository
{
    private readonly AppDbContext _context;

    public ProceduresRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Procedimento>> ListByEmpresaIdAsync(
        Guid empresaId,
        bool includeInactive,
        Guid? produtoAplicadoId,
        string? search,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Procedimentos
            .AsNoTracking()
            .Include(procedimento => procedimento.ProdutoAplicado)
            .Include(procedimento => procedimento.Itens)
                .ThenInclude(item => item.Produto)
            .Where(procedimento => procedimento.EmpresaId == empresaId);

        if (!includeInactive)
        {
            query = query.Where(procedimento => procedimento.Ativo);
        }

        if (produtoAplicadoId.HasValue)
        {
            query = query.Where(procedimento => procedimento.ProdutoAplicadoId == produtoAplicadoId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(procedimento => EF.Functions.Like(procedimento.Nome, pattern));
        }

        query = query.OrderBy(procedimento => procedimento.Nome);

        if (limit.HasValue)
        {
            query = query.Take(limit.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public Task<Procedimento?> GetByIdAndEmpresaIdWithDetailsAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return _context.Procedimentos
            .Include(procedimento => procedimento.ProdutoAplicado)
            .Include(procedimento => procedimento.Itens)
                .ThenInclude(item => item.Produto)
            .FirstOrDefaultAsync(
                procedimento => procedimento.Id == id && procedimento.EmpresaId == empresaId,
                cancellationToken);
    }

    public Task<bool> ExistsByNomeAsync(
        Guid empresaId,
        string nome,
        Guid? excludeId,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Procedimentos.AsNoTracking()
            .Where(procedimento => procedimento.EmpresaId == empresaId && procedimento.Nome == nome);

        if (excludeId.HasValue)
        {
            query = query.Where(procedimento => procedimento.Id != excludeId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    public Task<bool> ExistsActiveByIdAndEmpresaIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return _context.Procedimentos.AsNoTracking()
            .AnyAsync(
                procedimento => procedimento.Id == id
                    && procedimento.EmpresaId == empresaId
                    && procedimento.Ativo,
                cancellationToken);
    }

    public async Task<bool> AllActiveProductsExistAsync(
        Guid empresaId,
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken = default)
    {
        if (productIds.Count == 0)
        {
            return true;
        }

        var distinctIds = productIds.Distinct().ToList();
        var count = await _context.Produtos.AsNoTracking()
            .CountAsync(
                produto => produto.EmpresaId == empresaId
                    && produto.Ativo
                    && distinctIds.Contains(produto.Id),
                cancellationToken);

        return count == distinctIds.Count;
    }

    public async Task AddAsync(Procedimento procedimento, CancellationToken cancellationToken = default)
    {
        await _context.Procedimentos.AddAsync(procedimento, cancellationToken);
    }

    public void Update(Procedimento procedimento)
    {
        _context.Procedimentos.Update(procedimento);
    }
}
