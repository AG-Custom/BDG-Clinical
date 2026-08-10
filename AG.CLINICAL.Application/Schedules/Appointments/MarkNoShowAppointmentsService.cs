using AG.CLINICAL.Application.Abstractions.Persistence;
using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Identity.Abstractions;
using AG.CLINICAL.Application.Schedules.Abstractions;
using AG.CLINICAL.Application.Schedules.Dtos;
using AG.CLINICAL.Domain.Entities;
using AG.CLINICAL.Domain.Enums;
using AG.CLINICAL.Domain.Exceptions;

namespace AG.CLINICAL.Application.Schedules.Appointments;

public interface IMarkNoShowAppointmentsService
{
    Task<Result<AppointmentDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

public sealed class MarkNoShowAppointmentsService : IMarkNoShowAppointmentsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IAppointmentsRepository _appointmentsRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public MarkNoShowAppointmentsService(
        ICurrentTenantContext tenantContext,
        IAppointmentsRepository appointmentsRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _appointmentsRepository = appointmentsRepository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AppointmentDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;
        var agendamento = await _appointmentsRepository.GetByIdAndEmpresaIdWithDetailsAsync(
            id,
            empresaId,
            cancellationToken);

        if (agendamento is null)
        {
            return Result<AppointmentDto>.Failure("Agendamento não encontrado.");
        }

        var dadosAnteriores = AppointmentsAuditSerializer.Serialize(agendamento);

        try
        {
            agendamento.MarkAsNoShow();
            _appointmentsRepository.Update(agendamento);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditLogsService.RegisterEntityChangeAsync(
                empresaId,
                _tenantContext.UsuarioId,
                nameof(Agendamento),
                id,
                AcaoAuditoria.Editar,
                dadosAnteriores: dadosAnteriores,
                dadosNovos: AppointmentsAuditSerializer.Serialize(agendamento),
                cancellationToken: cancellationToken);

            return Result<AppointmentDto>.Success(AppointmentsMapper.Map(agendamento));
        }
        catch (DomainException exception)
        {
            return Result<AppointmentDto>.Failure(exception.Message);
        }
    }
}
