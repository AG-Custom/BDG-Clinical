using AG.CLINICAL.Application.Abstractions.Persistence;
using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.Application.Applications.Abstractions;
using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Core.Abstractions;
using AG.CLINICAL.Application.Identity;
using AG.CLINICAL.Application.Identity.Abstractions;
using AG.CLINICAL.Application.Patients.Abstractions;
using AG.CLINICAL.Application.Schedules.Abstractions;
using AG.CLINICAL.Application.Notifications.EmailOutbox;
using AG.CLINICAL.Application.Schedules.Dtos;
using AG.CLINICAL.Domain.Entities;
using AG.CLINICAL.Domain.Enums;
using AG.CLINICAL.Domain.Exceptions;

namespace AG.CLINICAL.Application.Schedules.Appointments;

public interface ICreateAppointmentsService
{
    Task<Result<AppointmentDto>> ExecuteAsync(
        CreateAppointmentRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class CreateAppointmentsService : ICreateAppointmentsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IAppointmentsRepository _appointmentsRepository;
    private readonly IPatientsRepository _patientsRepository;
    private readonly IEmployeesRepository _employeesRepository;
    private readonly IUnitsRepository _unitsRepository;
    private readonly IProceduresRepository _proceduresRepository;
    private readonly IUnitOperatingHoursRepository _operatingHoursRepository;
    private readonly IEmailOutboxEnqueueService _emailOutboxEnqueueService;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAppointmentsService(
        ICurrentTenantContext tenantContext,
        IAppointmentsRepository appointmentsRepository,
        IPatientsRepository patientsRepository,
        IEmployeesRepository employeesRepository,
        IUnitsRepository unitsRepository,
        IProceduresRepository proceduresRepository,
        IUnitOperatingHoursRepository operatingHoursRepository,
        IEmailOutboxEnqueueService emailOutboxEnqueueService,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _appointmentsRepository = appointmentsRepository;
        _patientsRepository = patientsRepository;
        _employeesRepository = employeesRepository;
        _unitsRepository = unitsRepository;
        _proceduresRepository = proceduresRepository;
        _operatingHoursRepository = operatingHoursRepository;
        _emailOutboxEnqueueService = emailOutboxEnqueueService;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AppointmentDto>> ExecuteAsync(
        CreateAppointmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;

        var validation = await AppointmentRequestValidator.ValidateAsync(
            empresaId,
            request,
            excludeAppointmentId: null,
            _appointmentsRepository,
            _patientsRepository,
            _employeesRepository,
            _unitsRepository,
            _proceduresRepository,
            _operatingHoursRepository,
            cancellationToken);

        if (validation.IsFailure)
        {
            return Result<AppointmentDto>.Failure(validation.Error!);
        }

        try
        {
            var data = validation.Value!;
            var agendamento = Agendamento.Create(
                empresaId,
                data.UnidadeId,
                data.PacienteId,
                data.FuncionarioId,
                data.ProcedimentoIds,
                data.Tipo,
                data.DataInicio,
                data.DataFim,
                data.Observacao,
                data.ExcecaoHorario,
                _tenantContext.UsuarioId,
                data.CompraPacienteId);

            await _appointmentsRepository.AddAsync(agendamento, cancellationToken);

            var paciente = await _patientsRepository.GetByIdAndEmpresaIdAsync(
                data.PacienteId,
                empresaId,
                cancellationToken);

            var pacienteEmail = paciente?.Email?.Trim();
            if (!string.IsNullOrWhiteSpace(pacienteEmail) && IdentityValidation.IsValidEmail(pacienteEmail))
            {
                await _emailOutboxEnqueueService.EnqueueAppointmentConfirmationAsync(
                    empresaId,
                    data.UnidadeId,
                    agendamento.Id,
                    pacienteEmail,
                    paciente!.Nome,
                    cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var persisted = await _appointmentsRepository.GetByIdAndEmpresaIdWithDetailsAsync(
                agendamento.Id,
                empresaId,
                cancellationToken);

            await _auditLogsService.RegisterEntityChangeAsync(
                empresaId,
                _tenantContext.UsuarioId,
                nameof(Agendamento),
                agendamento.Id,
                AcaoAuditoria.Criar,
                dadosNovos: AppointmentsAuditSerializer.Serialize(persisted ?? agendamento),
                cancellationToken: cancellationToken);

            return Result<AppointmentDto>.Success(AppointmentsMapper.Map(persisted ?? agendamento));
        }
        catch (DomainException exception)
        {
            return Result<AppointmentDto>.Failure(exception.Message);
        }
    }
}
