using AG.CLINICAL.Application.Schedules.Abstractions;
using AG.CLINICAL.Domain.Entities;
using AG.CLINICAL.Domain.Enums;
using AG.CLINICAL.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace AG.CLINICAL.Infra.Data.Repositories.Schedules;

public sealed class UnitOperatingHoursRepository : IUnitOperatingHoursRepository
{
    private readonly AppDbContext _context;

    public UnitOperatingHoursRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<HorarioFuncionamentoUnidade>> ListByUnitIdAsync(
        Guid empresaId,
        Guid unidadeId,
        bool includeInactive,
        CancellationToken cancellationToken = default)
    {
        var query = _context.HorariosFuncionamentoUnidade
            .AsNoTracking()
            .Where(h => h.EmpresaId == empresaId && h.UnidadeId == unidadeId);

        if (!includeInactive)
        {
            query = query.Where(h => h.Ativo);
        }

        return await query
            .OrderBy(h => h.DiaSemana)
            .ThenBy(h => h.HoraInicio)
            .ToListAsync(cancellationToken);
    }

    public Task<HorarioFuncionamentoUnidade?> GetByIdAndEmpresaIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return _context.HorariosFuncionamentoUnidade
            .FirstOrDefaultAsync(h => h.Id == id && h.EmpresaId == empresaId, cancellationToken);
    }

    public Task<bool> HasActiveOperatingHoursConfiguredAsync(
        Guid empresaId,
        Guid unidadeId,
        CancellationToken cancellationToken = default)
    {
        return _context.HorariosFuncionamentoUnidade.AnyAsync(
            h => h.EmpresaId == empresaId && h.UnidadeId == unidadeId && h.Ativo,
            cancellationToken);
    }

    public async Task<bool> HasMatchingOperatingHoursAsync(
        Guid empresaId,
        Guid unidadeId,
        DateTime dataInicio,
        DateTime dataFim,
        CancellationToken cancellationToken = default)
    {
        if (dataInicio.Date != dataFim.Date)
        {
            return false;
        }

        var diaSemana = (DiaSemana)(int)dataInicio.DayOfWeek;
        var horaInicio = TimeOnly.FromDateTime(dataInicio);
        var horaFim = TimeOnly.FromDateTime(dataFim);

        return await _context.HorariosFuncionamentoUnidade.AnyAsync(
            h => h.EmpresaId == empresaId
                && h.UnidadeId == unidadeId
                && h.Ativo
                && h.DiaSemana == diaSemana
                && h.HoraInicio <= horaInicio
                && h.HoraFim >= horaFim,
            cancellationToken);
    }

    public Task<bool> HasOverlappingOperatingHourAsync(
        Guid empresaId,
        Guid unidadeId,
        DiaSemana diaSemana,
        TimeOnly horaInicio,
        TimeOnly horaFim,
        Guid? excludeId = null,
        bool onlyActive = false,
        CancellationToken cancellationToken = default)
    {
        return _context.HorariosFuncionamentoUnidade.AnyAsync(
            h => h.EmpresaId == empresaId
                && h.UnidadeId == unidadeId
                && h.DiaSemana == diaSemana
                && (!excludeId.HasValue || h.Id != excludeId.Value)
                && (!onlyActive || h.Ativo)
                && h.HoraInicio < horaFim
                && horaInicio < h.HoraFim,
            cancellationToken);
    }

    public async Task AddAsync(HorarioFuncionamentoUnidade horario, CancellationToken cancellationToken = default)
    {
        await _context.HorariosFuncionamentoUnidade.AddAsync(horario, cancellationToken);
    }

    public void Update(HorarioFuncionamentoUnidade horario)
    {
        if (_context.Entry(horario).State == EntityState.Detached)
        {
            _context.HorariosFuncionamentoUnidade.Update(horario);
        }
    }
}
