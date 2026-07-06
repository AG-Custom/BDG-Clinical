using BGD.CLINICAL.Application.Patients.Abstractions;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace BGD.CLINICAL.Infra.Data.Repositories.Patients;

public sealed class PatientsRepository : IPatientsRepository
{
    private readonly AppDbContext _context;

    public PatientsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Paciente>> ListByEmpresaIdAsync(
        Guid empresaId,
        Guid? unidadeId,
        bool includeInactive,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Pacientes
            .AsNoTracking()
            .Include(paciente => paciente.UnidadesVinculadas)
                .ThenInclude(vinculo => vinculo.Unidade)
            .Where(paciente => paciente.EmpresaId == empresaId);

        if (unidadeId.HasValue)
        {
            query = query.Where(paciente =>
                paciente.UnidadeId == unidadeId.Value
                || paciente.UnidadesVinculadas.Any(vinculo => vinculo.UnidadeId == unidadeId.Value));
        }

        if (!includeInactive)
        {
            query = query.Where(paciente => paciente.Ativo);
        }

        return await query
            .OrderBy(paciente => paciente.Nome)
            .ToListAsync(cancellationToken);
    }

    public Task<Paciente?> GetByIdAndEmpresaIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return _context.Pacientes
            .FirstOrDefaultAsync(
                paciente => paciente.Id == id && paciente.EmpresaId == empresaId,
                cancellationToken);
    }

    public Task<Paciente?> GetByIdAndEmpresaIdWithDetailsAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return _context.Pacientes
            .Include(paciente => paciente.UnidadesVinculadas)
                .ThenInclude(vinculo => vinculo.Unidade)
            .FirstOrDefaultAsync(
                paciente => paciente.Id == id && paciente.EmpresaId == empresaId,
                cancellationToken);
    }

    public Task<bool> ExistsByCpfAsync(
        Guid empresaId,
        string cpf,
        Guid? excludeId,
        CancellationToken cancellationToken = default)
    {
        return _context.Pacientes.AnyAsync(
            paciente => paciente.EmpresaId == empresaId
                && paciente.Cpf == cpf
                && (!excludeId.HasValue || paciente.Id != excludeId.Value),
            cancellationToken);
    }

    public Task<bool> ExistsActiveUnidadeInEmpresaAsync(
        Guid unidadeId,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return _context.Unidades.AnyAsync(
            unidade => unidade.Id == unidadeId
                && unidade.EmpresaId == empresaId
                && unidade.Ativo,
            cancellationToken);
    }

    public async Task<bool> AllActiveUnitsExistInEmpresaAsync(
        Guid empresaId,
        IReadOnlyCollection<Guid> unidadeIds,
        CancellationToken cancellationToken = default)
    {
        if (unidadeIds.Count == 0)
        {
            return false;
        }

        var activeCount = await _context.Unidades.CountAsync(
            unidade => unidade.EmpresaId == empresaId
                && unidade.Ativo
                && unidadeIds.Contains(unidade.Id),
            cancellationToken);

        return activeCount == unidadeIds.Count;
    }

    public async Task AddAsync(Paciente paciente, CancellationToken cancellationToken = default)
    {
        await _context.Pacientes.AddAsync(paciente, cancellationToken);
    }

    public void Update(Paciente paciente)
    {
        var entry = _context.Entry(paciente);

        if (entry.State == EntityState.Detached)
        {
            _context.Pacientes.Attach(paciente);
            entry.State = EntityState.Modified;
        }

        foreach (var unidadeVinculada in paciente.UnidadesVinculadas)
        {
            EnsureUnidadeVinculadaTrackedCorrectly(unidadeVinculada);
        }
    }

    private void EnsureUnidadeVinculadaTrackedCorrectly(PacienteUnidade unidadeVinculada)
    {
        var itemEntry = _context.Entry(unidadeVinculada);

        if (itemEntry.State is EntityState.Added or EntityState.Deleted)
        {
            return;
        }

        if (_context.PacientesUnidade.Local.Any(tracked => tracked.Id == unidadeVinculada.Id))
        {
            if (itemEntry.State == EntityState.Modified && !UnidadeVinculadaExistsInDatabase(unidadeVinculada.Id))
            {
                _context.PacientesUnidade.Add(unidadeVinculada);
            }

            return;
        }

        if (UnidadeVinculadaExistsInDatabase(unidadeVinculada.Id))
        {
            if (itemEntry.State == EntityState.Detached)
            {
                _context.PacientesUnidade.Attach(unidadeVinculada);
            }

            itemEntry.State = EntityState.Modified;
            return;
        }

        _context.PacientesUnidade.Add(unidadeVinculada);
    }

    private bool UnidadeVinculadaExistsInDatabase(Guid id) =>
        _context.PacientesUnidade.Any(item => item.Id == id);
}
