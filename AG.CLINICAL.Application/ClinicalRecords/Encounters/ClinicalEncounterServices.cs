using AG.CLINICAL.Application.Abstractions.Persistence;
using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.Application.ClinicalRecords.Abstractions;
using AG.CLINICAL.Application.ClinicalRecords.Dtos;
using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Core.Abstractions;
using AG.CLINICAL.Application.Identity.Abstractions;
using AG.CLINICAL.Application.Patients.Abstractions;
using AG.CLINICAL.Domain.Entities;
using AG.CLINICAL.Domain.Enums;
using AG.CLINICAL.Domain.Exceptions;

namespace AG.CLINICAL.Application.ClinicalRecords.Encounters;

public interface IListClinicalEncountersService
{
    Task<Result<IReadOnlyList<ClinicalEncounterDto>>> ExecuteAsync(
        Guid pacienteId,
        CancellationToken cancellationToken = default);
}

public sealed class ListClinicalEncountersService : IListClinicalEncountersService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IClinicalEncountersRepository _encountersRepository;

    public ListClinicalEncountersService(
        ICurrentTenantContext tenantContext,
        IClinicalEncountersRepository encountersRepository)
    {
        _tenantContext = tenantContext;
        _encountersRepository = encountersRepository;
    }

    public async Task<Result<IReadOnlyList<ClinicalEncounterDto>>> ExecuteAsync(
        Guid pacienteId,
        CancellationToken cancellationToken = default)
    {
        var itens = await _encountersRepository.ListByPacienteAsync(
            _tenantContext.EmpresaId,
            pacienteId,
            includeInactive: false,
            cancellationToken);

        return Result<IReadOnlyList<ClinicalEncounterDto>>.Success(
            itens.Select(item => ClinicalRecordMapper.Map(item)).ToList());
    }
}

public interface IGetClinicalEncountersService
{
    Task<Result<ClinicalEncounterDto>> ExecuteAsync(Guid id, CancellationToken cancellationToken = default);
}

public sealed class GetClinicalEncountersService : IGetClinicalEncountersService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IClinicalEncountersRepository _encountersRepository;
    private readonly IMedicalRecordsRepository _medicalRecordsRepository;

    public GetClinicalEncountersService(
        ICurrentTenantContext tenantContext,
        IClinicalEncountersRepository encountersRepository,
        IMedicalRecordsRepository medicalRecordsRepository)
    {
        _tenantContext = tenantContext;
        _encountersRepository = encountersRepository;
        _medicalRecordsRepository = medicalRecordsRepository;
    }

    public async Task<Result<ClinicalEncounterDto>> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var atendimento = await _encountersRepository.GetByIdAndEmpresaIdAsync(id, _tenantContext.EmpresaId, cancellationToken);
        if (atendimento is null)
        {
            return Result<ClinicalEncounterDto>.Failure("Atendimento não encontrado.");
        }

        var timeline = await _medicalRecordsRepository.ListEventosByAtendimentoAsync(
            _tenantContext.EmpresaId,
            id,
            cancellationToken);

        return Result<ClinicalEncounterDto>.Success(
            ClinicalRecordMapper.Map(atendimento, timeline.Select(ClinicalRecordMapper.Map).ToList()));
    }
}

public interface ICreateClinicalEncountersService
{
    Task<Result<ClinicalEncounterDto>> ExecuteAsync(
        Guid pacienteId,
        CreateClinicalEncounterRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class CreateClinicalEncountersService : ICreateClinicalEncountersService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IPatientsRepository _patientsRepository;
    private readonly IUnitsRepository _unitsRepository;
    private readonly IEmployeesRepository _employeesRepository;
    private readonly IMedicalRecordsRepository _medicalRecordsRepository;
    private readonly IClinicalEncountersRepository _encountersRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly MedicalRecords.IGetOrCreateMedicalRecordsService _getOrCreate;

    public CreateClinicalEncountersService(
        ICurrentTenantContext tenantContext,
        IPatientsRepository patientsRepository,
        IUnitsRepository unitsRepository,
        IEmployeesRepository employeesRepository,
        IMedicalRecordsRepository medicalRecordsRepository,
        IClinicalEncountersRepository encountersRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork,
        MedicalRecords.IGetOrCreateMedicalRecordsService getOrCreate)
    {
        _tenantContext = tenantContext;
        _patientsRepository = patientsRepository;
        _unitsRepository = unitsRepository;
        _employeesRepository = employeesRepository;
        _medicalRecordsRepository = medicalRecordsRepository;
        _encountersRepository = encountersRepository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
        _getOrCreate = getOrCreate;
    }

    public async Task<Result<ClinicalEncounterDto>> ExecuteAsync(
        Guid pacienteId,
        CreateClinicalEncounterRequest request,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;
        var pasta = await _getOrCreate.ExecuteAsync(pacienteId, cancellationToken);
        if (pasta.IsFailure)
        {
            return Result<ClinicalEncounterDto>.Failure(pasta.Error!);
        }

        var paciente = await _patientsRepository.GetByIdAndEmpresaIdAsync(pacienteId, empresaId, cancellationToken);
        var unidade = await _unitsRepository.GetByIdAndEmpresaIdAsync(request.UnidadeId, empresaId, cancellationToken);
        var funcionario = await _employeesRepository.GetByIdAndEmpresaIdAsync(request.FuncionarioId, empresaId, cancellationToken);

        if (paciente is null)
        {
            return Result<ClinicalEncounterDto>.Failure("Paciente não encontrado.");
        }

        if (unidade is null || !unidade.Ativo)
        {
            return Result<ClinicalEncounterDto>.Failure("Unidade não encontrada.");
        }

        if (funcionario is null || !funcionario.Ativo)
        {
            return Result<ClinicalEncounterDto>.Failure("Profissional não encontrado.");
        }

        var prontuario = await _medicalRecordsRepository.GetByPacienteIdAsync(empresaId, pacienteId, cancellationToken);
        if (prontuario is null)
        {
            return Result<ClinicalEncounterDto>.Failure("Prontuário não encontrado.");
        }

        try
        {
            var atendimento = AtendimentoClinico.Create(
                empresaId,
                prontuario.Id,
                pacienteId,
                request.UnidadeId,
                request.FuncionarioId,
                request.DataInicio ?? DateTime.UtcNow,
                request.Observacao);

            await _encountersRepository.AddAsync(atendimento, cancellationToken);
            await _medicalRecordsRepository.AddEventoAsync(
                EventoClinico.Create(
                    empresaId,
                    pacienteId,
                    atendimento.Id,
                    TipoEventoClinico.AtendimentoCriado,
                    "Atendimento iniciado",
                    null,
                    nameof(AtendimentoClinico),
                    atendimento.Id,
                    request.FuncionarioId,
                    request.UnidadeId),
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _auditLogsService.RegisterEntityChangeAsync(
                empresaId,
                _tenantContext.UsuarioId,
                nameof(AtendimentoClinico),
                atendimento.Id,
                AcaoAuditoria.Criar,
                dadosNovos: ClinicalRecordAudit.Summary(new { atendimento.DataInicio, atendimento.Status }),
                cancellationToken: cancellationToken);

            var persisted = await _encountersRepository.GetByIdAndEmpresaIdAsync(atendimento.Id, empresaId, cancellationToken)
                ?? atendimento;
            return Result<ClinicalEncounterDto>.Success(ClinicalRecordMapper.Map(persisted));
        }
        catch (DomainException exception)
        {
            return Result<ClinicalEncounterDto>.Failure(exception.Message);
        }
    }
}

public interface IUpdateClinicalEncountersService
{
    Task<Result<ClinicalEncounterDto>> ExecuteAsync(
        Guid id,
        UpdateClinicalEncounterRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class UpdateClinicalEncountersService : IUpdateClinicalEncountersService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IClinicalEncountersRepository _encountersRepository;
    private readonly IUnitsRepository _unitsRepository;
    private readonly IEmployeesRepository _employeesRepository;
    private readonly IMedicalRecordsRepository _medicalRecordsRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateClinicalEncountersService(
        ICurrentTenantContext tenantContext,
        IClinicalEncountersRepository encountersRepository,
        IUnitsRepository unitsRepository,
        IEmployeesRepository employeesRepository,
        IMedicalRecordsRepository medicalRecordsRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _encountersRepository = encountersRepository;
        _unitsRepository = unitsRepository;
        _employeesRepository = employeesRepository;
        _medicalRecordsRepository = medicalRecordsRepository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ClinicalEncounterDto>> ExecuteAsync(
        Guid id,
        UpdateClinicalEncounterRequest request,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;
        var atendimento = await _encountersRepository.GetByIdAndEmpresaIdAsync(id, empresaId, cancellationToken);
        if (atendimento is null)
        {
            return Result<ClinicalEncounterDto>.Failure("Atendimento não encontrado.");
        }

        var unidade = await _unitsRepository.GetByIdAndEmpresaIdAsync(request.UnidadeId, empresaId, cancellationToken);
        var funcionario = await _employeesRepository.GetByIdAndEmpresaIdAsync(request.FuncionarioId, empresaId, cancellationToken);
        if (unidade is null || funcionario is null)
        {
            return Result<ClinicalEncounterDto>.Failure("Unidade ou profissional não encontrado.");
        }

        try
        {
            var anteriores = ClinicalRecordAudit.Summary(new
            {
                atendimento.UnidadeId,
                atendimento.FuncionarioId,
                atendimento.DataInicio,
                atendimento.Observacao
            });

            atendimento.UpdateDetails(request.UnidadeId, request.FuncionarioId, request.DataInicio, request.Observacao);
            _encountersRepository.Update(atendimento);

            await _medicalRecordsRepository.AddEventoAsync(
                EventoClinico.Create(
                    empresaId,
                    atendimento.PacienteId,
                    atendimento.Id,
                    TipoEventoClinico.AtendimentoAtualizado,
                    "Atendimento alterado",
                    "Dados do atendimento atualizados",
                    nameof(AtendimentoClinico),
                    atendimento.Id,
                    request.FuncionarioId,
                    request.UnidadeId,
                    anteriores,
                    ClinicalRecordAudit.Summary(new
                    {
                        atendimento.UnidadeId,
                        atendimento.FuncionarioId,
                        atendimento.DataInicio,
                        atendimento.Observacao
                    })),
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _auditLogsService.RegisterEntityChangeAsync(
                empresaId,
                _tenantContext.UsuarioId,
                nameof(AtendimentoClinico),
                atendimento.Id,
                AcaoAuditoria.Editar,
                anteriores,
                ClinicalRecordAudit.Summary(new { atendimento.UnidadeId, atendimento.FuncionarioId, atendimento.DataInicio }),
                cancellationToken);

            var persisted = await _encountersRepository.GetByIdAndEmpresaIdAsync(id, empresaId, cancellationToken)
                ?? atendimento;
            return Result<ClinicalEncounterDto>.Success(ClinicalRecordMapper.Map(persisted));
        }
        catch (DomainException exception)
        {
            return Result<ClinicalEncounterDto>.Failure(exception.Message);
        }
    }
}

public interface IFinalizeClinicalEncountersService
{
    Task<Result<ClinicalEncounterDto>> ExecuteAsync(Guid id, CancellationToken cancellationToken = default);
}

public sealed class FinalizeClinicalEncountersService : IFinalizeClinicalEncountersService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IClinicalEncountersRepository _encountersRepository;
    private readonly IMedicalRecordsRepository _medicalRecordsRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public FinalizeClinicalEncountersService(
        ICurrentTenantContext tenantContext,
        IClinicalEncountersRepository encountersRepository,
        IMedicalRecordsRepository medicalRecordsRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _encountersRepository = encountersRepository;
        _medicalRecordsRepository = medicalRecordsRepository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ClinicalEncounterDto>> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;
        var atendimento = await _encountersRepository.GetByIdAndEmpresaIdAsync(id, empresaId, cancellationToken);
        if (atendimento is null)
        {
            return Result<ClinicalEncounterDto>.Failure("Atendimento não encontrado.");
        }

        atendimento.FinalizeEncounter();
        _encountersRepository.Update(atendimento);
        await _medicalRecordsRepository.AddEventoAsync(
            EventoClinico.Create(
                empresaId,
                atendimento.PacienteId,
                atendimento.Id,
                TipoEventoClinico.AtendimentoFinalizado,
                "Atendimento finalizado",
                null,
                nameof(AtendimentoClinico),
                atendimento.Id,
                atendimento.FuncionarioId,
                atendimento.UnidadeId),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _auditLogsService.RegisterEntityChangeAsync(
            empresaId,
            _tenantContext.UsuarioId,
            nameof(AtendimentoClinico),
            atendimento.Id,
            AcaoAuditoria.Editar,
            dadosNovos: ClinicalRecordAudit.Summary(new { atendimento.Status, atendimento.DataFim }),
            cancellationToken: cancellationToken);

        return Result<ClinicalEncounterDto>.Success(ClinicalRecordMapper.Map(atendimento));
    }
}
