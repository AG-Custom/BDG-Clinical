using AG.CLINICAL.Application.Abstractions.Persistence;
using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.Application.Applications.Abstractions;
using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Core.Abstractions;
using AG.CLINICAL.Application.Identity.Abstractions;
using AG.CLINICAL.Application.Patients.Abstractions;
using AG.CLINICAL.Application.Schedules.Abstractions;
using AG.CLINICAL.Application.Schedules.Dtos;
using AG.CLINICAL.Domain.Entities;
using AG.CLINICAL.Domain.Enums;
using AG.CLINICAL.Domain.Exceptions;

namespace AG.CLINICAL.Application.Schedules.Appointments;

public interface IUpdateAppointmentsService
{
    Task<Result<AppointmentDto>> ExecuteAsync(
        Guid id,
        UpdateAppointmentRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class UpdateAppointmentsService : IUpdateAppointmentsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IAppointmentsRepository _appointmentsRepository;
    private readonly IPatientsRepository _patientsRepository;
    private readonly IEmployeesRepository _employeesRepository;
    private readonly IUnitsRepository _unitsRepository;
    private readonly IProceduresRepository _proceduresRepository;
    private readonly IAppointmentTagsRepository _appointmentTagsRepository;
    private readonly IUnitOperatingHoursRepository _operatingHoursRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAppointmentsService(
        ICurrentTenantContext tenantContext,
        IAppointmentsRepository appointmentsRepository,
        IPatientsRepository patientsRepository,
        IEmployeesRepository employeesRepository,
        IUnitsRepository unitsRepository,
        IProceduresRepository proceduresRepository,
        IAppointmentTagsRepository appointmentTagsRepository,
        IUnitOperatingHoursRepository operatingHoursRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _appointmentsRepository = appointmentsRepository;
        _patientsRepository = patientsRepository;
        _employeesRepository = employeesRepository;
        _unitsRepository = unitsRepository;
        _proceduresRepository = proceduresRepository;
        _appointmentTagsRepository = appointmentTagsRepository;
        _operatingHoursRepository = operatingHoursRepository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AppointmentDto>> ExecuteAsync(
        Guid id,
        UpdateAppointmentRequest request,
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

        var validation = await AppointmentRequestValidator.ValidateAsync(
            empresaId,
            id,
            request,
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

        IReadOnlyList<Guid>? tagIdsParaPersistir = null;

        if (request.TagIds is not null)
        {
            var tagIdsResult = AppointmentRequestValidator.ResolveTagIds(request.TagIds);
            if (tagIdsResult.IsFailure)
            {
                return Result<AppointmentDto>.Failure(tagIdsResult.Error!);
            }

            var tagsValidation = await AppointmentRequestValidator.ValidateTagsAsync(
                empresaId,
                tagIdsResult.Value!,
                agendamento.GetTagIds(),
                _appointmentTagsRepository,
                cancellationToken);

            if (tagsValidation.IsFailure)
            {
                return Result<AppointmentDto>.Failure(tagsValidation.Error!);
            }

            tagIdsParaPersistir = tagIdsResult.Value!;
        }

        try
        {
            var data = validation.Value!;
            agendamento.UpdateDetails(
                data.UnidadeId,
                data.PacienteId,
                data.FuncionarioId,
                data.ProcedimentoIds,
                data.Tipo,
                data.DataInicio,
                data.DataFim,
                data.Observacao,
                data.ExcecaoHorario,
                data.CompraPacienteId);

            if (tagIdsParaPersistir is not null)
            {
                agendamento.SetTags(tagIdsParaPersistir);
            }

            _appointmentsRepository.Update(agendamento);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var persisted = await _appointmentsRepository.GetByIdAndEmpresaIdWithDetailsAsync(
                id,
                empresaId,
                cancellationToken);

            await _auditLogsService.RegisterEntityChangeAsync(
                empresaId,
                _tenantContext.UsuarioId,
                nameof(Agendamento),
                id,
                AcaoAuditoria.Editar,
                dadosAnteriores: dadosAnteriores,
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
