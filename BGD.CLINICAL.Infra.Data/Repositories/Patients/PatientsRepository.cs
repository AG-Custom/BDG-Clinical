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
            .Where(paciente => paciente.EmpresaId == empresaId);

        if (unidadeId.HasValue)
        {
            query = query.Where(paciente => paciente.UnidadeId == unidadeId.Value);
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

    public async Task AddAsync(Paciente paciente, CancellationToken cancellationToken = default)
    {
        await _context.Pacientes.AddAsync(paciente, cancellationToken);
    }

    public void Update(Paciente paciente)
    {
        var entry = _context.Entry(paciente);

        if (entry.State == EntityState.Detached)
        {
            _context.Pacientes.Update(paciente);
        }
    }
}
