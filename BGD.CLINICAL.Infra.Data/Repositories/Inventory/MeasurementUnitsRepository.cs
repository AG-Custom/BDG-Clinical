using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using BGD.CLINICAL.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace BGD.CLINICAL.Infra.Data.Repositories.Inventory;

public sealed class MeasurementUnitsRepository : IMeasurementUnitsRepository
{
    private readonly AppDbContext _context;

    public MeasurementUnitsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<UnidadeMedida>> ListByEmpresaIdAsync(
        Guid empresaId,
        bool includeInactive,
        TipoUnidadeMedida? tipo,
        string? search,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var query = _context.UnidadesMedida
            .AsNoTracking()
            .Where(unidade => unidade.EmpresaId == empresaId);

        if (!includeInactive)
        {
            query = query.Where(unidade => unidade.Ativo);
        }

        if (tipo.HasValue)
        {
            query = query.Where(unidade => unidade.Tipo == tipo.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";

            query = query.Where(unidade =>
                EF.Functions.Like(unidade.Nome, pattern) ||
                EF.Functions.Like(unidade.Sigla, pattern));
        }

        var orderedQuery = query.OrderBy(unidade => unidade.Nome);

        if (limit.HasValue)
        {
            return await orderedQuery
                .Take(limit.Value)
                .ToListAsync(cancellationToken);
        }

        return await orderedQuery.ToListAsync(cancellationToken);
    }

    public Task<UnidadeMedida?> GetByIdAndEmpresaIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return _context.UnidadesMedida
            .FirstOrDefaultAsync(
                unidade => unidade.Id == id && unidade.EmpresaId == empresaId,
                cancellationToken);
    }

    public Task<bool> ExistsByNomeAsync(
        Guid empresaId,
        string nome,
        Guid? excludeId,
        CancellationToken cancellationToken = default)
    {
        var normalizedNome = nome.Trim().ToUpperInvariant();

        return _context.UnidadesMedida.AnyAsync(
            unidade => unidade.EmpresaId == empresaId
                && unidade.Nome.ToUpper() == normalizedNome
                && (!excludeId.HasValue || unidade.Id != excludeId.Value),
            cancellationToken);
    }

    public Task<bool> ExistsBySiglaAsync(
        Guid empresaId,
        string sigla,
        Guid? excludeId,
        CancellationToken cancellationToken = default)
    {
        var normalizedSigla = sigla.Trim().ToUpperInvariant();

        return _context.UnidadesMedida.AnyAsync(
            unidade => unidade.EmpresaId == empresaId
                && unidade.Sigla.ToUpper() == normalizedSigla
                && (!excludeId.HasValue || unidade.Id != excludeId.Value),
            cancellationToken);
    }

    public Task<bool> ExistsActiveByIdAndEmpresaIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return _context.UnidadesMedida.AnyAsync(
            unidade => unidade.Id == id && unidade.EmpresaId == empresaId && unidade.Ativo,
            cancellationToken);
    }

    public Task<bool> HasActiveProductsAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return _context.Produtos.AnyAsync(
            produto => produto.UnidadeMedidaId == id
                && produto.EmpresaId == empresaId
                && produto.Ativo,
            cancellationToken);
    }

    public async Task AddAsync(UnidadeMedida unidadeMedida, CancellationToken cancellationToken = default)
    {
        await _context.UnidadesMedida.AddAsync(unidadeMedida, cancellationToken);
    }

    public void Update(UnidadeMedida unidadeMedida)
    {
        var entry = _context.Entry(unidadeMedida);

        if (entry.State == EntityState.Detached)
        {
            _context.UnidadesMedida.Update(unidadeMedida);
        }
    }
}
