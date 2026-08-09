using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Schedules.Abstractions;
using AG.CLINICAL.Application.Schedules.Dtos;

namespace AG.CLINICAL.Application.Schedules.Appointments;

public interface IGetAppointmentsService
{
    Task<Result<AppointmentDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

public sealed class GetAppointmentsService : IGetAppointmentsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IAppointmentsRepository _appointmentsRepository;

    public GetAppointmentsService(
        ICurrentTenantContext tenantContext,
        IAppointmentsRepository appointmentsRepository)
    {
        _tenantContext = tenantContext;
        _appointmentsRepository = appointmentsRepository;
    }

    public async Task<Result<AppointmentDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var agendamento = await _appointmentsRepository.GetByIdAndEmpresaIdWithDetailsAsync(
            id,
            _tenantContext.EmpresaId,
            cancellationToken);

        if (agendamento is null)
        {
            return Result<AppointmentDto>.Failure("Agendamento não encontrado.");
        }

        return Result<AppointmentDto>.Success(AppointmentsMapper.Map(agendamento));
    }
}
