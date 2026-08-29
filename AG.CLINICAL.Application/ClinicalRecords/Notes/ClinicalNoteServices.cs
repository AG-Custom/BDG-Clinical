using AG.CLINICAL.Application.Abstractions.Persistence;
using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.Application.ClinicalRecords.Abstractions;
using AG.CLINICAL.Application.ClinicalRecords.Dtos;
using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Identity.Abstractions;
using AG.CLINICAL.Domain.Entities;
using AG.CLINICAL.Domain.Enums;
using AG.CLINICAL.Domain.Exceptions;

namespace AG.CLINICAL.Application.ClinicalRecords.Notes;

public interface IListClinicalNotesService
{
    Task<Result<IReadOnlyList<ClinicalNoteDto>>> ExecuteAsync(Guid atendimentoId, CancellationToken cancellationToken = default);
}

public sealed class ListClinicalNotesService : IListClinicalNotesService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IClinicalNotesRepository _notesRepository;

    public ListClinicalNotesService(ICurrentTenantContext tenantContext, IClinicalNotesRepository notesRepository)
    {
        _tenantContext = tenantContext;
        _notesRepository = notesRepository;
    }

    public async Task<Result<IReadOnlyList<ClinicalNoteDto>>> ExecuteAsync(
        Guid atendimentoId,
        CancellationToken cancellationToken = default)
    {
        var itens = await _notesRepository.ListByAtendimentoAsync(_tenantContext.EmpresaId, atendimentoId, cancellationToken);
        return Result<IReadOnlyList<ClinicalNoteDto>>.Success(itens.Select(ClinicalRecordMapper.Map).ToList());
    }
}

public interface ICreateClinicalNotesService
{
    Task<Result<ClinicalNoteDto>> ExecuteAsync(
        Guid atendimentoId,
        CreateClinicalNoteRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class CreateClinicalNotesService : ICreateClinicalNotesService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IClinicalEncountersRepository _encountersRepository;
    private readonly IClinicalNotesRepository _notesRepository;
    private readonly IMedicalRecordsRepository _medicalRecordsRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateClinicalNotesService(
        ICurrentTenantContext tenantContext,
        IClinicalEncountersRepository encountersRepository,
        IClinicalNotesRepository notesRepository,
        IMedicalRecordsRepository medicalRecordsRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _encountersRepository = encountersRepository;
        _notesRepository = notesRepository;
        _medicalRecordsRepository = medicalRecordsRepository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ClinicalNoteDto>> ExecuteAsync(
        Guid atendimentoId,
        CreateClinicalNoteRequest request,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;
        var atendimento = await _encountersRepository.GetByIdAndEmpresaIdAsync(atendimentoId, empresaId, cancellationToken);
        if (atendimento is null)
        {
            return Result<ClinicalNoteDto>.Failure("Atendimento não encontrado.");
        }

        try
        {
            var tipo = ClinicalRecordEnums.Parse<TipoAnotacaoClinica>(request.Tipo, "Tipo de anotação inválido.");
            var anotacao = AnotacaoClinica.Create(
                empresaId,
                atendimento.Id,
                atendimento.PacienteId,
                atendimento.UnidadeId,
                atendimento.FuncionarioId,
                tipo,
                request.Texto,
                request.Data ?? DateTime.UtcNow);

            await _notesRepository.AddAsync(anotacao, cancellationToken);
            await _medicalRecordsRepository.AddEventoAsync(
                EventoClinico.Create(
                    empresaId,
                    atendimento.PacienteId,
                    atendimento.Id,
                    TipoEventoClinico.AnotacaoCriada,
                    "Anotação clínica registrada",
                    tipo.ToString(),
                    nameof(AnotacaoClinica),
                    anotacao.Id,
                    atendimento.FuncionarioId,
                    atendimento.UnidadeId),
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _auditLogsService.RegisterEntityChangeAsync(
                empresaId,
                _tenantContext.UsuarioId,
                nameof(AnotacaoClinica),
                anotacao.Id,
                AcaoAuditoria.Criar,
                dadosNovos: ClinicalRecordAudit.Summary(new { anotacao.Tipo }),
                cancellationToken: cancellationToken);

            var persisted = await _notesRepository.GetByIdAndEmpresaIdAsync(anotacao.Id, empresaId, cancellationToken)
                ?? anotacao;
            return Result<ClinicalNoteDto>.Success(ClinicalRecordMapper.Map(persisted));
        }
        catch (DomainException exception)
        {
            return Result<ClinicalNoteDto>.Failure(exception.Message);
        }
    }
}

public interface IUpdateClinicalNotesService
{
    Task<Result<ClinicalNoteDto>> ExecuteAsync(
        Guid id,
        UpdateClinicalNoteRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class UpdateClinicalNotesService : IUpdateClinicalNotesService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IClinicalNotesRepository _notesRepository;
    private readonly IMedicalRecordsRepository _medicalRecordsRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateClinicalNotesService(
        ICurrentTenantContext tenantContext,
        IClinicalNotesRepository notesRepository,
        IMedicalRecordsRepository medicalRecordsRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _notesRepository = notesRepository;
        _medicalRecordsRepository = medicalRecordsRepository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ClinicalNoteDto>> ExecuteAsync(
        Guid id,
        UpdateClinicalNoteRequest request,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;
        var anotacao = await _notesRepository.GetByIdAndEmpresaIdAsync(id, empresaId, cancellationToken);
        if (anotacao is null)
        {
            return Result<ClinicalNoteDto>.Failure("Anotação não encontrada.");
        }

        try
        {
            var tipo = ClinicalRecordEnums.Parse<TipoAnotacaoClinica>(request.Tipo, "Tipo de anotação inválido.");
            var anteriores = ClinicalRecordAudit.Summary(new { anotacao.Tipo, anotacao.Data });
            anotacao.UpdateDetails(tipo, request.Texto, request.Data);
            _notesRepository.Update(anotacao);

            await _medicalRecordsRepository.AddEventoAsync(
                EventoClinico.Create(
                    empresaId,
                    anotacao.PacienteId,
                    anotacao.AtendimentoClinicoId,
                    TipoEventoClinico.AnotacaoAtualizada,
                    "Anotação clínica alterada",
                    tipo.ToString(),
                    nameof(AnotacaoClinica),
                    anotacao.Id,
                    anotacao.FuncionarioId,
                    anotacao.UnidadeId,
                    anteriores,
                    ClinicalRecordAudit.Summary(new { anotacao.Tipo, anotacao.Data })),
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _auditLogsService.RegisterEntityChangeAsync(
                empresaId,
                _tenantContext.UsuarioId,
                nameof(AnotacaoClinica),
                anotacao.Id,
                AcaoAuditoria.Editar,
                anteriores,
                ClinicalRecordAudit.Summary(new { anotacao.Tipo }),
                cancellationToken);

            return Result<ClinicalNoteDto>.Success(ClinicalRecordMapper.Map(anotacao));
        }
        catch (DomainException exception)
        {
            return Result<ClinicalNoteDto>.Failure(exception.Message);
        }
    }
}
