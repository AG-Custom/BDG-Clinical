using AG.CLINICAL.Application.Abstractions.Persistence;
using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.Application.Applications.Abstractions;
using AG.CLINICAL.Application.ClinicalRecords.Abstractions;
using AG.CLINICAL.Application.ClinicalRecords.Dtos;
using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Core.Abstractions;
using AG.CLINICAL.Application.Identity.Abstractions;
using AG.CLINICAL.Application.Packages.Abstractions;
using AG.CLINICAL.Application.Patients.Abstractions;
using AG.CLINICAL.Application.Schedules.Abstractions;
using AG.CLINICAL.Domain.Entities;
using AG.CLINICAL.Domain.Enums;
using AG.CLINICAL.Domain.Exceptions;
using AG.CLINICAL.Domain.Services;

namespace AG.CLINICAL.Application.ClinicalRecords.MedicalRecords;

public interface IGetOrCreateMedicalRecordsService
{
    Task<Result<MedicalRecordDto>> ExecuteAsync(Guid pacienteId, CancellationToken cancellationToken = default);
}

public sealed class GetOrCreateMedicalRecordsService : IGetOrCreateMedicalRecordsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IPatientsRepository _patientsRepository;
    private readonly IMedicalRecordsRepository _medicalRecordsRepository;
    private readonly IUnitOfWork _unitOfWork;

    public GetOrCreateMedicalRecordsService(
        ICurrentTenantContext tenantContext,
        IPatientsRepository patientsRepository,
        IMedicalRecordsRepository medicalRecordsRepository,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _patientsRepository = patientsRepository;
        _medicalRecordsRepository = medicalRecordsRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<MedicalRecordDto>> ExecuteAsync(Guid pacienteId, CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;
        var paciente = await _patientsRepository.GetByIdAndEmpresaIdWithDetailsAsync(pacienteId, empresaId, cancellationToken);
        if (paciente is null)
        {
            return Result<MedicalRecordDto>.Failure("Paciente não encontrado.");
        }

        var prontuario = await _medicalRecordsRepository.GetByPacienteIdAsync(empresaId, pacienteId, cancellationToken);
        if (prontuario is null)
        {
            try
            {
                prontuario = Prontuario.Create(empresaId, pacienteId);
                await _medicalRecordsRepository.AddAsync(prontuario, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                prontuario = await _medicalRecordsRepository.GetByPacienteIdAsync(empresaId, pacienteId, cancellationToken)
                    ?? prontuario;
            }
            catch (DomainException exception)
            {
                return Result<MedicalRecordDto>.Failure(exception.Message);
            }
        }

        return Result<MedicalRecordDto>.Success(
            ClinicalRecordMapper.Map(prontuario, DateOnly.FromDateTime(DateTime.UtcNow)));
    }
}

public interface IUpdateMedicalRecordsService
{
    Task<Result<MedicalRecordDto>> ExecuteAsync(
        Guid pacienteId,
        UpdateMedicalRecordRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class UpdateMedicalRecordsService : IUpdateMedicalRecordsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IGetOrCreateMedicalRecordsService _getOrCreate;
    private readonly IMedicalRecordsRepository _medicalRecordsRepository;
    private readonly IPatientsRepository _patientsRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateMedicalRecordsService(
        ICurrentTenantContext tenantContext,
        IGetOrCreateMedicalRecordsService getOrCreate,
        IMedicalRecordsRepository medicalRecordsRepository,
        IPatientsRepository patientsRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _getOrCreate = getOrCreate;
        _medicalRecordsRepository = medicalRecordsRepository;
        _patientsRepository = patientsRepository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<MedicalRecordDto>> ExecuteAsync(
        Guid pacienteId,
        UpdateMedicalRecordRequest request,
        CancellationToken cancellationToken = default)
    {
        var created = await _getOrCreate.ExecuteAsync(pacienteId, cancellationToken);
        if (created.IsFailure)
        {
            return created;
        }

        var empresaId = _tenantContext.EmpresaId;
        var prontuario = await _medicalRecordsRepository.GetByPacienteIdAsync(empresaId, pacienteId, cancellationToken);
        if (prontuario is null)
        {
            return Result<MedicalRecordDto>.Failure("Prontuário não encontrado.");
        }

        try
        {
            var anteriores = ClinicalRecordAudit.Summary(new { prontuario.Alergias, prontuario.Alertas, prontuario.Observacao });
            prontuario.UpdateDetails(request.Alergias, request.Alertas, request.Observacao);
            _medicalRecordsRepository.Update(prontuario);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _auditLogsService.RegisterEntityChangeAsync(
                empresaId,
                _tenantContext.UsuarioId,
                nameof(Prontuario),
                prontuario.Id,
                AcaoAuditoria.Editar,
                anteriores,
                ClinicalRecordAudit.Summary(new { prontuario.Alergias, prontuario.Alertas, prontuario.Observacao }),
                cancellationToken);

            var persisted = await _medicalRecordsRepository.GetByPacienteIdAsync(empresaId, pacienteId, cancellationToken)
                ?? prontuario;
            return Result<MedicalRecordDto>.Success(
                ClinicalRecordMapper.Map(persisted, DateOnly.FromDateTime(DateTime.UtcNow)));
        }
        catch (DomainException exception)
        {
            return Result<MedicalRecordDto>.Failure(exception.Message);
        }
    }
}

public interface IGetMedicalRecordSummariesService
{
    Task<Result<MedicalRecordSummaryDto>> ExecuteAsync(Guid pacienteId, CancellationToken cancellationToken = default);
}

public sealed class GetMedicalRecordSummariesService : IGetMedicalRecordSummariesService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IGetOrCreateMedicalRecordsService _getOrCreate;
    private readonly IMedicalRecordsRepository _medicalRecordsRepository;
    private readonly IBodyAssessmentsRepository _bodyAssessmentsRepository;
    private readonly IAppointmentsRepository _appointmentsRepository;
    private readonly IPatientPurchasesRepository _patientPurchasesRepository;
    private readonly IPatientApplicationsRepository _patientApplicationsRepository;

    public GetMedicalRecordSummariesService(
        ICurrentTenantContext tenantContext,
        IGetOrCreateMedicalRecordsService getOrCreate,
        IMedicalRecordsRepository medicalRecordsRepository,
        IBodyAssessmentsRepository bodyAssessmentsRepository,
        IAppointmentsRepository appointmentsRepository,
        IPatientPurchasesRepository patientPurchasesRepository,
        IPatientApplicationsRepository patientApplicationsRepository)
    {
        _tenantContext = tenantContext;
        _getOrCreate = getOrCreate;
        _medicalRecordsRepository = medicalRecordsRepository;
        _bodyAssessmentsRepository = bodyAssessmentsRepository;
        _appointmentsRepository = appointmentsRepository;
        _patientPurchasesRepository = patientPurchasesRepository;
        _patientApplicationsRepository = patientApplicationsRepository;
    }

    public async Task<Result<MedicalRecordSummaryDto>> ExecuteAsync(
        Guid pacienteId,
        CancellationToken cancellationToken = default)
    {
        var prontuarioResult = await _getOrCreate.ExecuteAsync(pacienteId, cancellationToken);
        if (prontuarioResult.IsFailure)
        {
            return Result<MedicalRecordSummaryDto>.Failure(prontuarioResult.Error!);
        }

        var empresaId = _tenantContext.EmpresaId;
        var avaliacao = await _bodyAssessmentsRepository.GetLatestByPacienteAsync(empresaId, pacienteId, cancellationToken);
        var agora = DateTime.UtcNow;
        var agendamentos = await _appointmentsRepository.ListByEmpresaIdAsync(
            empresaId,
            null,
            null,
            pacienteId,
            null,
            agora,
            agora.AddMonths(6),
            cancellationToken);

        var proximo = agendamentos
            .Where(item => item.Status is StatusAgendamento.Agendado or StatusAgendamento.Confirmado)
            .OrderBy(item => item.DataInicio)
            .FirstOrDefault();

        var compras = await _patientPurchasesRepository.ListActiveByPacienteAsync(empresaId, pacienteId, cancellationToken);
        var aplicacoes = await _patientApplicationsRepository.ListByEmpresaIdAsync(
            empresaId,
            pacienteId,
            null,
            null,
            null,
            null,
            false,
            null,
            null,
            5,
            cancellationToken);

        var timeline = await _medicalRecordsRepository.ListEventosByPacienteAsync(empresaId, pacienteId, 20, cancellationToken);

        return Result<MedicalRecordSummaryDto>.Success(new MedicalRecordSummaryDto(
            prontuarioResult.Value!,
            avaliacao is null ? null : ClinicalRecordMapper.Map(avaliacao),
            proximo is null
                ? null
                : new NextAppointmentSummaryDto(
                    proximo.Id,
                    proximo.DataInicio,
                    proximo.Tipo.ToString(),
                    proximo.Status.ToString(),
                    proximo.Funcionario?.Nome,
                    proximo.Unidade?.Nome),
            compras.Select(item => new ActivePackageSummaryDto(item.Id, item.Pacote?.Nome ?? string.Empty, item.DataCompra)).ToList(),
            aplicacoes.Select(item => new ApplicationSummaryDto(
                item.Id,
                item.DataAplicacao,
                item.Procedimento?.Nome,
                item.Produto?.Nome)).ToList(),
            timeline.Select(ClinicalRecordMapper.Map).ToList()));
    }
}
