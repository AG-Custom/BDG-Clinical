using AG.CLINICAL.Application.Abstractions.Persistence;
using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.Application.ClinicalRecords.Abstractions;
using AG.CLINICAL.Application.ClinicalRecords.Dtos;
using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Identity.Abstractions;
using AG.CLINICAL.Domain.Entities;
using AG.CLINICAL.Domain.Enums;
using AG.CLINICAL.Domain.Exceptions;

namespace AG.CLINICAL.Application.ClinicalRecords.Anamneses;

public interface IListAnamneseTemplatesService
{
    Task<Result<IReadOnlyList<AnamneseTemplateDto>>> ExecuteAsync(bool includeInactive, CancellationToken cancellationToken = default);
}

public sealed class ListAnamneseTemplatesService : IListAnamneseTemplatesService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IAnamneseTemplatesRepository _repository;

    public ListAnamneseTemplatesService(ICurrentTenantContext tenantContext, IAnamneseTemplatesRepository repository)
    {
        _tenantContext = tenantContext;
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<AnamneseTemplateDto>>> ExecuteAsync(
        bool includeInactive,
        CancellationToken cancellationToken = default)
    {
        var itens = await _repository.ListByEmpresaIdAsync(_tenantContext.EmpresaId, includeInactive, cancellationToken);
        return Result<IReadOnlyList<AnamneseTemplateDto>>.Success(itens.Select(ClinicalRecordMapper.Map).ToList());
    }
}

public interface IGetAnamneseTemplatesService
{
    Task<Result<AnamneseTemplateDto>> ExecuteAsync(Guid id, CancellationToken cancellationToken = default);
}

public sealed class GetAnamneseTemplatesService : IGetAnamneseTemplatesService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IAnamneseTemplatesRepository _repository;

    public GetAnamneseTemplatesService(ICurrentTenantContext tenantContext, IAnamneseTemplatesRepository repository)
    {
        _tenantContext = tenantContext;
        _repository = repository;
    }

    public async Task<Result<AnamneseTemplateDto>> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var modelo = await _repository.GetByIdAndEmpresaIdAsync(id, _tenantContext.EmpresaId, cancellationToken);
        return modelo is null
            ? Result<AnamneseTemplateDto>.Failure("Modelo de anamnese não encontrado.")
            : Result<AnamneseTemplateDto>.Success(ClinicalRecordMapper.Map(modelo));
    }
}

public interface ICreateAnamneseTemplatesService
{
    Task<Result<AnamneseTemplateDto>> ExecuteAsync(CreateAnamneseTemplateRequest request, CancellationToken cancellationToken = default);
}

public sealed class CreateAnamneseTemplatesService : ICreateAnamneseTemplatesService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IAnamneseTemplatesRepository _repository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAnamneseTemplatesService(
        ICurrentTenantContext tenantContext,
        IAnamneseTemplatesRepository repository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _repository = repository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AnamneseTemplateDto>> ExecuteAsync(
        CreateAnamneseTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        if (await _repository.ExistsByNomeAsync(_tenantContext.EmpresaId, request.Nome, null, cancellationToken))
        {
            return Result<AnamneseTemplateDto>.Failure("Já existe um modelo com este nome.");
        }

        try
        {
            var modelo = ModeloAnamnese.Create(
                _tenantContext.EmpresaId,
                request.Nome,
                request.Especialidade,
                ClinicalRecordJson.Serialize(request.Campos ?? []));

            await _repository.AddAsync(modelo, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _auditLogsService.RegisterEntityChangeAsync(
                _tenantContext.EmpresaId,
                _tenantContext.UsuarioId,
                nameof(ModeloAnamnese),
                modelo.Id,
                AcaoAuditoria.Criar,
                dadosNovos: ClinicalRecordAudit.Summary(new { modelo.Nome }),
                cancellationToken: cancellationToken);

            return Result<AnamneseTemplateDto>.Success(ClinicalRecordMapper.Map(modelo));
        }
        catch (DomainException exception)
        {
            return Result<AnamneseTemplateDto>.Failure(exception.Message);
        }
    }
}

public interface IUpdateAnamneseTemplatesService
{
    Task<Result<AnamneseTemplateDto>> ExecuteAsync(
        Guid id,
        UpdateAnamneseTemplateRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class UpdateAnamneseTemplatesService : IUpdateAnamneseTemplatesService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IAnamneseTemplatesRepository _repository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAnamneseTemplatesService(
        ICurrentTenantContext tenantContext,
        IAnamneseTemplatesRepository repository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _repository = repository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AnamneseTemplateDto>> ExecuteAsync(
        Guid id,
        UpdateAnamneseTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var modelo = await _repository.GetByIdAndEmpresaIdAsync(id, _tenantContext.EmpresaId, cancellationToken);
        if (modelo is null)
        {
            return Result<AnamneseTemplateDto>.Failure("Modelo de anamnese não encontrado.");
        }

        if (await _repository.ExistsByNomeAsync(_tenantContext.EmpresaId, request.Nome, id, cancellationToken))
        {
            return Result<AnamneseTemplateDto>.Failure("Já existe um modelo com este nome.");
        }

        try
        {
            var anteriores = ClinicalRecordAudit.Summary(new { modelo.Nome });
            modelo.UpdateDetails(request.Nome, request.Especialidade, ClinicalRecordJson.Serialize(request.Campos ?? []));
            _repository.Update(modelo);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _auditLogsService.RegisterEntityChangeAsync(
                _tenantContext.EmpresaId,
                _tenantContext.UsuarioId,
                nameof(ModeloAnamnese),
                modelo.Id,
                AcaoAuditoria.Editar,
                anteriores,
                ClinicalRecordAudit.Summary(new { modelo.Nome }),
                cancellationToken);

            return Result<AnamneseTemplateDto>.Success(ClinicalRecordMapper.Map(modelo));
        }
        catch (DomainException exception)
        {
            return Result<AnamneseTemplateDto>.Failure(exception.Message);
        }
    }
}

public interface IDeactivateAnamneseTemplatesService
{
    Task<Result<AnamneseTemplateDto>> ExecuteAsync(Guid id, CancellationToken cancellationToken = default);
}

public sealed class DeactivateAnamneseTemplatesService : IDeactivateAnamneseTemplatesService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IAnamneseTemplatesRepository _repository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateAnamneseTemplatesService(
        ICurrentTenantContext tenantContext,
        IAnamneseTemplatesRepository repository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _repository = repository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AnamneseTemplateDto>> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var modelo = await _repository.GetByIdAndEmpresaIdAsync(id, _tenantContext.EmpresaId, cancellationToken);
        if (modelo is null)
        {
            return Result<AnamneseTemplateDto>.Failure("Modelo de anamnese não encontrado.");
        }

        modelo.Deactivate();
        _repository.Update(modelo);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _auditLogsService.RegisterEntityChangeAsync(
            _tenantContext.EmpresaId,
            _tenantContext.UsuarioId,
            nameof(ModeloAnamnese),
            modelo.Id,
            AcaoAuditoria.Excluir,
            dadosNovos: ClinicalRecordAudit.Summary(new { modelo.Ativo }),
            cancellationToken: cancellationToken);

        return Result<AnamneseTemplateDto>.Success(ClinicalRecordMapper.Map(modelo));
    }
}

public interface IListAnamneseRecordsService
{
    Task<Result<IReadOnlyList<AnamneseRecordDto>>> ExecuteAsync(Guid atendimentoId, CancellationToken cancellationToken = default);
}

public sealed class ListAnamneseRecordsService : IListAnamneseRecordsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IAnamneseRecordsRepository _repository;

    public ListAnamneseRecordsService(ICurrentTenantContext tenantContext, IAnamneseRecordsRepository repository)
    {
        _tenantContext = tenantContext;
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<AnamneseRecordDto>>> ExecuteAsync(
        Guid atendimentoId,
        CancellationToken cancellationToken = default)
    {
        var itens = await _repository.ListByAtendimentoAsync(_tenantContext.EmpresaId, atendimentoId, cancellationToken);
        return Result<IReadOnlyList<AnamneseRecordDto>>.Success(itens.Select(ClinicalRecordMapper.Map).ToList());
    }
}

public interface ICreateAnamneseRecordsService
{
    Task<Result<AnamneseRecordDto>> ExecuteAsync(
        Guid atendimentoId,
        CreateAnamneseRecordRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class CreateAnamneseRecordsService : ICreateAnamneseRecordsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IClinicalEncountersRepository _encountersRepository;
    private readonly IAnamneseTemplatesRepository _templatesRepository;
    private readonly IAnamneseRecordsRepository _recordsRepository;
    private readonly IMedicalRecordsRepository _medicalRecordsRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAnamneseRecordsService(
        ICurrentTenantContext tenantContext,
        IClinicalEncountersRepository encountersRepository,
        IAnamneseTemplatesRepository templatesRepository,
        IAnamneseRecordsRepository recordsRepository,
        IMedicalRecordsRepository medicalRecordsRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _encountersRepository = encountersRepository;
        _templatesRepository = templatesRepository;
        _recordsRepository = recordsRepository;
        _medicalRecordsRepository = medicalRecordsRepository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AnamneseRecordDto>> ExecuteAsync(
        Guid atendimentoId,
        CreateAnamneseRecordRequest request,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;
        var atendimento = await _encountersRepository.GetByIdAndEmpresaIdAsync(atendimentoId, empresaId, cancellationToken);
        var modelo = await _templatesRepository.GetByIdAndEmpresaIdAsync(request.ModeloAnamneseId, empresaId, cancellationToken);

        if (atendimento is null)
        {
            return Result<AnamneseRecordDto>.Failure("Atendimento não encontrado.");
        }

        if (modelo is null || !modelo.Ativo)
        {
            return Result<AnamneseRecordDto>.Failure("Modelo de anamnese não encontrado.");
        }

        try
        {
            var registro = RegistroAnamnese.Create(
                empresaId,
                atendimento.Id,
                atendimento.PacienteId,
                atendimento.UnidadeId,
                atendimento.FuncionarioId,
                modelo.Id,
                modelo.SchemaJson,
                request.RespostasJson);

            await _recordsRepository.AddAsync(registro, cancellationToken);
            await _medicalRecordsRepository.AddEventoAsync(
                EventoClinico.Create(
                    empresaId,
                    atendimento.PacienteId,
                    atendimento.Id,
                    TipoEventoClinico.AnamneseCriada,
                    "Anamnese preenchida",
                    modelo.Nome,
                    nameof(RegistroAnamnese),
                    registro.Id,
                    atendimento.FuncionarioId,
                    atendimento.UnidadeId),
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _auditLogsService.RegisterEntityChangeAsync(
                empresaId,
                _tenantContext.UsuarioId,
                nameof(RegistroAnamnese),
                registro.Id,
                AcaoAuditoria.Criar,
                dadosNovos: ClinicalRecordAudit.Summary(new { modelo.Nome, registro.VersaoAtual }),
                cancellationToken: cancellationToken);

            var persisted = await _recordsRepository.GetByIdAndEmpresaIdAsync(registro.Id, empresaId, cancellationToken)
                ?? registro;
            return Result<AnamneseRecordDto>.Success(ClinicalRecordMapper.Map(persisted));
        }
        catch (DomainException exception)
        {
            return Result<AnamneseRecordDto>.Failure(exception.Message);
        }
    }
}

public interface IUpdateAnamneseRecordsService
{
    Task<Result<AnamneseRecordDto>> ExecuteAsync(
        Guid id,
        UpdateAnamneseRecordRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class UpdateAnamneseRecordsService : IUpdateAnamneseRecordsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IAnamneseRecordsRepository _recordsRepository;
    private readonly IMedicalRecordsRepository _medicalRecordsRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAnamneseRecordsService(
        ICurrentTenantContext tenantContext,
        IAnamneseRecordsRepository recordsRepository,
        IMedicalRecordsRepository medicalRecordsRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _recordsRepository = recordsRepository;
        _medicalRecordsRepository = medicalRecordsRepository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AnamneseRecordDto>> ExecuteAsync(
        Guid id,
        UpdateAnamneseRecordRequest request,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;
        var registro = await _recordsRepository.GetByIdAndEmpresaIdAsync(id, empresaId, cancellationToken);
        if (registro is null)
        {
            return Result<AnamneseRecordDto>.Failure("Anamnese não encontrada.");
        }

        try
        {
            var versaoAnterior = registro.VersaoAtual;
            registro.UpdateAnswers(registro.FuncionarioId, request.RespostasJson, request.ResumoAlteracao);
            _recordsRepository.Update(registro);

            await _medicalRecordsRepository.AddEventoAsync(
                EventoClinico.Create(
                    empresaId,
                    registro.PacienteId,
                    registro.AtendimentoClinicoId,
                    TipoEventoClinico.AnamneseAtualizada,
                    "Anamnese atualizada",
                    $"Versão {versaoAnterior} → {registro.VersaoAtual}",
                    nameof(RegistroAnamnese),
                    registro.Id,
                    registro.FuncionarioId,
                    registro.UnidadeId,
                    ClinicalRecordAudit.Summary(new { Versao = versaoAnterior }),
                    ClinicalRecordAudit.Summary(new { registro.VersaoAtual })),
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _auditLogsService.RegisterEntityChangeAsync(
                empresaId,
                _tenantContext.UsuarioId,
                nameof(RegistroAnamnese),
                registro.Id,
                AcaoAuditoria.Editar,
                ClinicalRecordAudit.Summary(new { Versao = versaoAnterior }),
                ClinicalRecordAudit.Summary(new { registro.VersaoAtual }),
                cancellationToken);

            var persisted = await _recordsRepository.GetByIdAndEmpresaIdAsync(id, empresaId, cancellationToken)
                ?? registro;
            return Result<AnamneseRecordDto>.Success(ClinicalRecordMapper.Map(persisted));
        }
        catch (DomainException exception)
        {
            return Result<AnamneseRecordDto>.Failure(exception.Message);
        }
    }
}
