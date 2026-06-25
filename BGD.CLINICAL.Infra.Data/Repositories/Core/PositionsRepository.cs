using BGD.CLINICAL.Application.Core.Abstractions;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace BGD.CLINICAL.Infra.Data.Repositories.Core;

public sealed class PositionsRepository : IPositionsRepository
{
    private readonly AppDbContext _context;

    public PositionsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Cargo>> ListByEmpresaIdAsync(
        Guid empresaId,
        bool includeInactive,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Cargos
            .AsNoTracking()
            .Where(cargo => cargo.EmpresaId == empresaId);

        if (!includeInactive)
        {
            query = query.Where(cargo => cargo.Ativo);
        }

        return await query
            .OrderBy(cargo => cargo.Nome)
            .ToListAsync(cancellationToken);
    }

    public Task<Cargo?> GetByIdAndEmpresaIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return _context.Cargos
            .FirstOrDefaultAsync(
                cargo => cargo.Id == id && cargo.EmpresaId == empresaId,
                cancellationToken);
    }

    public Task<bool> ExistsByNomeAsync(
        Guid empresaId,
        string nome,
        Guid? excludeId,
        CancellationToken cancellationToken = default)
    {
        var normalizedNome = nome.Trim().ToUpperInvariant();

        return _context.Cargos.AnyAsync(
            cargo => cargo.EmpresaId == empresaId
                && cargo.Nome.ToUpper() == normalizedNome
                && (!excludeId.HasValue || cargo.Id != excludeId.Value),
            cancellationToken);
    }

    public Task<bool> ExistsActiveByIdAndEmpresaIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return _context.Cargos.AnyAsync(
            cargo => cargo.Id == id && cargo.EmpresaId == empresaId && cargo.Ativo,
            cancellationToken);
    }

    public async Task AddAsync(Cargo cargo, CancellationToken cancellationToken = default)
    {
        await _context.Cargos.AddAsync(cargo, cancellationToken);
    }

    public void Update(Cargo cargo)
    {
        _context.Cargos.Update(cargo);
    }
}
