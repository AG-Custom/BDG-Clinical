using BGD.CLINICAL.Application.Patients.Abstractions;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace BGD.CLINICAL.Infra.Data.Repositories.Patients;

public sealed class SymptomsRepository : ISymptomsRepository
{
    private readonly AppDbContext _context;

    public SymptomsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Sintoma>> ListByEmpresaIdAsync(
        Guid empresaId,
        bool includeInactive,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Sintomas
            .AsNoTracking()
            .Where(sintoma => sintoma.EmpresaId == empresaId);

        if (!includeInactive)
        {
            query = query.Where(sintoma => sintoma.Ativo);
        }

        return await query
            .OrderBy(sintoma => sintoma.Nome)
            .ToListAsync(cancellationToken);
    }

    public Task<Sintoma?> GetByIdAndEmpresaIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return _context.Sintomas
            .FirstOrDefaultAsync(
                sintoma => sintoma.Id == id && sintoma.EmpresaId == empresaId,
                cancellationToken);
    }

    public Task<bool> ExistsByNomeAsync(
        Guid empresaId,
        string nome,
        Guid? excludeId,
        CancellationToken cancellationToken = default)
    {
        var normalizedNome = nome.Trim().ToUpperInvariant();

        return _context.Sintomas.AnyAsync(
            sintoma => sintoma.EmpresaId == empresaId
                && sintoma.Nome.ToUpper() == normalizedNome
                && (!excludeId.HasValue || sintoma.Id != excludeId.Value),
            cancellationToken);
    }

    public async Task<bool> AllExistActiveByIdsAsync(
        Guid empresaId,
        IReadOnlyList<Guid> sintomaIds,
        CancellationToken cancellationToken = default)
    {
        if (sintomaIds.Count == 0)
        {
            return true;
        }

        var distinctIds = sintomaIds.Distinct().ToList();
        var count = await _context.Sintomas.CountAsync(
            sintoma => distinctIds.Contains(sintoma.Id)
                && sintoma.EmpresaId == empresaId
                && sintoma.Ativo,
            cancellationToken);

        return count == distinctIds.Count;
    }

    public async Task AddAsync(Sintoma sintoma, CancellationToken cancellationToken = default)
    {
        await _context.Sintomas.AddAsync(sintoma, cancellationToken);
    }

    public void Update(Sintoma sintoma)
    {
        var entry = _context.Entry(sintoma);

        if (entry.State == EntityState.Detached)
        {
            _context.Sintomas.Update(sintoma);
        }
    }
}
