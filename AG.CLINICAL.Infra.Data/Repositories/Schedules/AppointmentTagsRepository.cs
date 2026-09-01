using AG.CLINICAL.Application.Schedules.Abstractions;
using AG.CLINICAL.Domain.Entities;
using AG.CLINICAL.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace AG.CLINICAL.Infra.Data.Repositories.Schedules;

public sealed class AppointmentTagsRepository : IAppointmentTagsRepository
{
    private readonly AppDbContext _context;

    public AppointmentTagsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<TagAgendamento>> ListByEmpresaIdAsync(
        Guid empresaId,
        bool includeInactive,
        CancellationToken cancellationToken = default)
    {
        var query = _context.TagsAgendamento
            .AsNoTracking()
            .Where(tag => tag.EmpresaId == empresaId);

        if (!includeInactive)
        {
            query = query.Where(tag => tag.Ativo);
        }

        return await query
            .OrderBy(tag => tag.Nome)
            .ToListAsync(cancellationToken);
    }

    public Task<TagAgendamento?> GetByIdAndEmpresaIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return _context.TagsAgendamento
            .FirstOrDefaultAsync(
                tag => tag.Id == id && tag.EmpresaId == empresaId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<TagAgendamento>> ListByIdsAndEmpresaIdAsync(
        Guid empresaId,
        IReadOnlyList<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
        {
            return [];
        }

        var distinctIds = ids.Distinct().ToList();

        return await _context.TagsAgendamento
            .AsNoTracking()
            .Where(tag => tag.EmpresaId == empresaId && distinctIds.Contains(tag.Id))
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsByNomeAsync(
        Guid empresaId,
        string nome,
        Guid? excludeId,
        CancellationToken cancellationToken = default)
    {
        var normalizedNome = nome.Trim().ToUpperInvariant();

        return _context.TagsAgendamento.AnyAsync(
            tag => tag.EmpresaId == empresaId
                && tag.Nome.ToUpper() == normalizedNome
                && (!excludeId.HasValue || tag.Id != excludeId.Value),
            cancellationToken);
    }

    public async Task AddAsync(TagAgendamento tag, CancellationToken cancellationToken = default)
    {
        await _context.TagsAgendamento.AddAsync(tag, cancellationToken);
    }

    public void Update(TagAgendamento tag)
    {
        var entry = _context.Entry(tag);

        if (entry.State == EntityState.Detached)
        {
            _context.TagsAgendamento.Update(tag);
        }
    }
}
