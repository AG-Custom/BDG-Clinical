using AG.CLINICAL.Domain.Entities;
using AG.CLINICAL.Domain.Enums;

namespace AG.CLINICAL.Application.Schedules.Abstractions;

public interface IUnitOperatingHoursRepository
{
    Task<IReadOnlyList<HorarioFuncionamentoUnidade>> ListByUnitIdAsync(
        Guid empresaId,
        Guid unidadeId,
        bool includeInactive,
        CancellationToken cancellationToken = default);

    Task<HorarioFuncionamentoUnidade?> GetByIdAndEmpresaIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default);

    Task<bool> HasActiveOperatingHoursConfiguredAsync(
        Guid empresaId,
        Guid unidadeId,
        CancellationToken cancellationToken = default);

    Task<bool> HasMatchingOperatingHoursAsync(
        Guid empresaId,
        Guid unidadeId,
        DateTime dataInicio,
        DateTime dataFim,
        CancellationToken cancellationToken = default);

    Task<bool> HasOverlappingOperatingHourAsync(
        Guid empresaId,
        Guid unidadeId,
        DiaSemana diaSemana,
        TimeOnly horaInicio,
        TimeOnly horaFim,
        Guid? excludeId = null,
        bool onlyActive = false,
        CancellationToken cancellationToken = default);

    Task AddAsync(HorarioFuncionamentoUnidade horario, CancellationToken cancellationToken = default);

    void Update(HorarioFuncionamentoUnidade horario);
}
