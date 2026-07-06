using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;

namespace BGD.CLINICAL.Application.Schedules.Abstractions;

public interface IAppointmentsRepository
{
    Task<IReadOnlyList<Agendamento>> ListByEmpresaIdAsync(
        Guid empresaId,
        Guid? unidadeId,
        Guid? funcionarioId,
        Guid? pacienteId,
        StatusAgendamento? status,
        DateTime? dataInicioFrom,
        DateTime? dataInicioTo,
        CancellationToken cancellationToken = default);

    Task<Agendamento?> GetByIdAndEmpresaIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default);

    Task<Agendamento?> GetByIdAndEmpresaIdWithDetailsAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default);

    Task<bool> HasScheduleBlockAsync(
        Guid empresaId,
        Guid funcionarioId,
        Guid unidadeId,
        DateTime dataInicio,
        DateTime dataFim,
        CancellationToken cancellationToken = default);

    Task<bool> HasMatchingAvailabilityAsync(
        Guid empresaId,
        Guid funcionarioId,
        Guid unidadeId,
        DateTime dataInicio,
        DateTime dataFim,
        CancellationToken cancellationToken = default);

    Task<bool> HasActiveAvailabilityConfiguredAsync(
        Guid empresaId,
        Guid funcionarioId,
        Guid unidadeId,
        CancellationToken cancellationToken = default);

    Task AddAsync(Agendamento agendamento, CancellationToken cancellationToken = default);

    void Update(Agendamento agendamento);
}
