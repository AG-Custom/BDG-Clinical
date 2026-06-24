using BGD.CLINICAL.Application.Core.Abstractions;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace BGD.CLINICAL.Infra.Data.Repositories.Core;

public sealed class UnitsRepository : IUnitsRepository
{
    private readonly AppDbContext _context;

    public UnitsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Unidade>> ListByEmpresaIdAsync(
        Guid empresaId,
        bool includeInactive,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Unidades
            .AsNoTracking()
            .Where(unidade => unidade.EmpresaId == empresaId);

        if (!includeInactive)
        {
            query = query.Where(unidade => unidade.Ativo);
        }

        return await query
            .OrderBy(unidade => unidade.Nome)
            .ToListAsync(cancellationToken);
    }

    public Task<Unidade?> GetByIdAndEmpresaIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return _context.Unidades
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

        return _context.Unidades.AnyAsync(
            unidade => unidade.EmpresaId == empresaId
                && unidade.Nome.ToUpper() == normalizedNome
                && (!excludeId.HasValue || unidade.Id != excludeId.Value),
            cancellationToken);
    }

    public async Task AddAsync(Unidade unidade, CancellationToken cancellationToken = default)
    {
        await _context.Unidades.AddAsync(unidade, cancellationToken);
    }

    public void Update(Unidade unidade)
    {
        _context.Unidades.Update(unidade);
    }
}
