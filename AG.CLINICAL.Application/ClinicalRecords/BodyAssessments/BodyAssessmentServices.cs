using AG.CLINICAL.Application.Abstractions.Persistence;
using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.Application.ClinicalRecords.Abstractions;
using AG.CLINICAL.Application.ClinicalRecords.Dtos;
using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Identity.Abstractions;
using AG.CLINICAL.Domain.Entities;
using AG.CLINICAL.Domain.Enums;
using AG.CLINICAL.Domain.Exceptions;
using AG.CLINICAL.Domain.Services;

namespace AG.CLINICAL.Application.ClinicalRecords.BodyAssessments;

public interface IListBodyAssessmentsService
{
    Task<Result<IReadOnlyList<BodyAssessmentDto>>> ExecuteAsync(
        Guid pacienteId,
        Guid? atendimentoId,
        CancellationToken cancellationToken = default);
}

public sealed class ListBodyAssessmentsService : IListBodyAssessmentsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IBodyAssessmentsRepository _repository;

    public ListBodyAssessmentsService(ICurrentTenantContext tenantContext, IBodyAssessmentsRepository repository)
    {
        _tenantContext = tenantContext;
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<BodyAssessmentDto>>> ExecuteAsync(
        Guid pacienteId,
        Guid? atendimentoId,
        CancellationToken cancellationToken = default)
    {
        var itens = atendimentoId.HasValue
            ? await _repository.ListByAtendimentoAsync(_tenantContext.EmpresaId, atendimentoId.Value, cancellationToken)
            : await _repository.ListByPacienteAsync(_tenantContext.EmpresaId, pacienteId, cancellationToken);

        return Result<IReadOnlyList<BodyAssessmentDto>>.Success(itens.Select(ClinicalRecordMapper.Map).ToList());
    }
}

public interface IGetBodyAssessmentsService
{
    Task<Result<BodyAssessmentDto>> ExecuteAsync(Guid id, CancellationToken cancellationToken = default);
}

public sealed class GetBodyAssessmentsService : IGetBodyAssessmentsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IBodyAssessmentsRepository _repository;

    public GetBodyAssessmentsService(ICurrentTenantContext tenantContext, IBodyAssessmentsRepository repository)
    {
        _tenantContext = tenantContext;
        _repository = repository;
    }

    public async Task<Result<BodyAssessmentDto>> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var avaliacao = await _repository.GetByIdAndEmpresaIdAsync(id, _tenantContext.EmpresaId, cancellationToken);
        return avaliacao is null
            ? Result<BodyAssessmentDto>.Failure("Avaliação corporal não encontrada.")
            : Result<BodyAssessmentDto>.Success(ClinicalRecordMapper.Map(avaliacao));
    }
}

public interface ICreateBodyAssessmentsService
{
    Task<Result<BodyAssessmentDto>> ExecuteAsync(
        Guid atendimentoId,
        UpsertBodyAssessmentRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class CreateBodyAssessmentsService : ICreateBodyAssessmentsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IClinicalEncountersRepository _encountersRepository;
    private readonly IBodyAssessmentsRepository _repository;
    private readonly IMedicalRecordsRepository _medicalRecordsRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBodyAssessmentsService(
        ICurrentTenantContext tenantContext,
        IClinicalEncountersRepository encountersRepository,
        IBodyAssessmentsRepository repository,
        IMedicalRecordsRepository medicalRecordsRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _encountersRepository = encountersRepository;
        _repository = repository;
        _medicalRecordsRepository = medicalRecordsRepository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BodyAssessmentDto>> ExecuteAsync(
        Guid atendimentoId,
        UpsertBodyAssessmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;
        var atendimento = await _encountersRepository.GetByIdAndEmpresaIdAsync(atendimentoId, empresaId, cancellationToken);
        if (atendimento is null)
        {
            return Result<BodyAssessmentDto>.Failure("Atendimento não encontrado.");
        }

        try
        {
            var avaliacao = AvaliacaoCorporal.Create(
                empresaId,
                atendimento.Id,
                atendimento.PacienteId,
                atendimento.UnidadeId,
                atendimento.FuncionarioId,
                request.DataAvaliacao ?? DateTime.UtcNow);

            ApplyMeasurements(avaliacao, atendimento.Paciente, request);
            await _repository.AddAsync(avaliacao, cancellationToken);
            await _medicalRecordsRepository.AddEventoAsync(
                EventoClinico.Create(
                    empresaId,
                    atendimento.PacienteId,
                    atendimento.Id,
                    TipoEventoClinico.AvaliacaoCriada,
                    "Avaliação corporal criada",
                    avaliacao.PesoKg is null ? null : $"Peso {avaliacao.PesoKg} kg",
                    nameof(AvaliacaoCorporal),
                    avaliacao.Id,
                    atendimento.FuncionarioId,
                    atendimento.UnidadeId),
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _auditLogsService.RegisterEntityChangeAsync(
                empresaId,
                _tenantContext.UsuarioId,
                nameof(AvaliacaoCorporal),
                avaliacao.Id,
                AcaoAuditoria.Criar,
                dadosNovos: ClinicalRecordAudit.Summary(new { avaliacao.PesoKg, avaliacao.Imc }),
                cancellationToken: cancellationToken);

            var persisted = await _repository.GetByIdAndEmpresaIdAsync(avaliacao.Id, empresaId, cancellationToken)
                ?? avaliacao;
            return Result<BodyAssessmentDto>.Success(ClinicalRecordMapper.Map(persisted));
        }
        catch (DomainException exception)
        {
            return Result<BodyAssessmentDto>.Failure(exception.Message);
        }
    }

    internal static void ApplyMeasurements(
        AvaliacaoCorporal avaliacao,
        Paciente? paciente,
        UpsertBodyAssessmentRequest request)
    {
        var cintura = request.CinturaCm ?? request.Circunferencias?.Cintura;
        var quadril = request.QuadrilCm ?? request.Circunferencias?.Quadril;
        var imc = CalculoCorporal.CalcularImc(request.PesoKg, request.AlturaCm);
        var protocolo = ClinicalRecordEnums.ParseOptional<ProtocoloPregaCutanea>(request.ProtocoloPrega);
        var idade = CalculoCorporal.CalcularIdade(
            paciente?.DataNascimento,
            DateOnly.FromDateTime(request.DataAvaliacao ?? DateTime.UtcNow));

        decimal? percentualGorda = request.Bioimpedancia?.PercentualMassaGorda;
        decimal? massaGorda = request.Bioimpedancia?.MassaGordaKg;
        decimal? percentualMagra = request.Bioimpedancia?.PercentualMassaMagra;
        decimal? massaMagra = request.Bioimpedancia?.MassaMagraKg;
        var percentualAgua = request.Bioimpedancia?.PercentualAgua;

        if (protocolo.HasValue && paciente?.Sexo is not null && idade is not null && request.PesoKg is not null)
        {
            var pregas = request.Pregas;
            var resultado = CalculoCorporal.CalcularJacksonPollock(
                protocolo.Value,
                paciente.Sexo.Value,
                idade.Value,
                request.PesoKg.Value,
                pregas?.AxilarMedia,
                pregas?.Triceps,
                pregas?.Subescapular,
                pregas?.SupraIliaca,
                pregas?.Torax,
                pregas?.Abdominal,
                pregas?.Coxa);

            if (resultado is not null)
            {
                percentualGorda = resultado.PercentualMassaGorda;
                massaGorda = resultado.MassaGordaKg;
                percentualMagra = resultado.PercentualMassaMagra;
                massaMagra = resultado.MassaMagraKg;
            }
        }

        avaliacao.ApplyMeasurements(
            request.AlturaCm,
            request.PesoKg,
            imc,
            CalculoCorporal.ClassificarImc(imc),
            CalculoCorporal.CalcularPesoIdealLorentz(request.AlturaCm, paciente?.Sexo),
            cintura,
            quadril,
            CalculoCorporal.CalcularRelacaoCinturaQuadril(cintura, quadril),
            percentualGorda,
            massaGorda,
            percentualMagra,
            massaMagra,
            percentualAgua,
            protocolo,
            request.Bioimpedancia is null ? null : ClinicalRecordJson.Serialize(request.Bioimpedancia),
            request.Circunferencias is null ? null : ClinicalRecordJson.Serialize(request.Circunferencias),
            request.Pregas is null ? null : ClinicalRecordJson.Serialize(request.Pregas),
            request.Observacao);
    }
}

public interface IUpdateBodyAssessmentsService
{
    Task<Result<BodyAssessmentDto>> ExecuteAsync(
        Guid id,
        UpsertBodyAssessmentRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class UpdateBodyAssessmentsService : IUpdateBodyAssessmentsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IBodyAssessmentsRepository _repository;
    private readonly IMedicalRecordsRepository _medicalRecordsRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBodyAssessmentsService(
        ICurrentTenantContext tenantContext,
        IBodyAssessmentsRepository repository,
        IMedicalRecordsRepository medicalRecordsRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _repository = repository;
        _medicalRecordsRepository = medicalRecordsRepository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BodyAssessmentDto>> ExecuteAsync(
        Guid id,
        UpsertBodyAssessmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;
        var avaliacao = await _repository.GetByIdAndEmpresaIdAsync(id, empresaId, cancellationToken);
        if (avaliacao is null)
        {
            return Result<BodyAssessmentDto>.Failure("Avaliação corporal não encontrada.");
        }

        try
        {
            var anteriores = ClinicalRecordAudit.Summary(new { avaliacao.PesoKg, avaliacao.Imc });
            if (request.DataAvaliacao.HasValue)
            {
                avaliacao.ApplyMeasurements(
                    avaliacao.AlturaCm,
                    avaliacao.PesoKg,
                    avaliacao.Imc,
                    avaliacao.ClassificacaoImc,
                    avaliacao.PesoIdealKg,
                    avaliacao.CinturaCm,
                    avaliacao.QuadrilCm,
                    avaliacao.RelacaoCinturaQuadril,
                    avaliacao.PercentualMassaGorda,
                    avaliacao.MassaGordaKg,
                    avaliacao.PercentualMassaMagra,
                    avaliacao.MassaMagraKg,
                    avaliacao.PercentualAgua,
                    avaliacao.ProtocoloPrega,
                    avaliacao.BioimpedanciaJson,
                    avaliacao.CircunferenciasJson,
                    avaliacao.PregasJson,
                    avaliacao.Observacao);
            }

            CreateBodyAssessmentsService.ApplyMeasurements(avaliacao, avaliacao.Paciente, request);
            _repository.Update(avaliacao);

            await _medicalRecordsRepository.AddEventoAsync(
                EventoClinico.Create(
                    empresaId,
                    avaliacao.PacienteId,
                    avaliacao.AtendimentoClinicoId,
                    TipoEventoClinico.AvaliacaoAtualizada,
                    "Avaliação corporal corrigida",
                    anteriores,
                    nameof(AvaliacaoCorporal),
                    avaliacao.Id,
                    avaliacao.FuncionarioId,
                    avaliacao.UnidadeId,
                    anteriores,
                    ClinicalRecordAudit.Summary(new { avaliacao.PesoKg, avaliacao.Imc })),
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _auditLogsService.RegisterEntityChangeAsync(
                empresaId,
                _tenantContext.UsuarioId,
                nameof(AvaliacaoCorporal),
                avaliacao.Id,
                AcaoAuditoria.Editar,
                anteriores,
                ClinicalRecordAudit.Summary(new { avaliacao.PesoKg, avaliacao.Imc }),
                cancellationToken);

            return Result<BodyAssessmentDto>.Success(ClinicalRecordMapper.Map(avaliacao));
        }
        catch (DomainException exception)
        {
            return Result<BodyAssessmentDto>.Failure(exception.Message);
        }
    }
}

public interface IGetBodyEvolutionService
{
    Task<Result<IReadOnlyList<BodyEvolutionPointDto>>> ExecuteAsync(
        Guid pacienteId,
        CancellationToken cancellationToken = default);
}

public sealed class GetBodyEvolutionService : IGetBodyEvolutionService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IBodyAssessmentsRepository _repository;

    public GetBodyEvolutionService(ICurrentTenantContext tenantContext, IBodyAssessmentsRepository repository)
    {
        _tenantContext = tenantContext;
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<BodyEvolutionPointDto>>> ExecuteAsync(
        Guid pacienteId,
        CancellationToken cancellationToken = default)
    {
        var itens = await _repository.ListByPacienteAsync(_tenantContext.EmpresaId, pacienteId, cancellationToken);
        var pontos = itens
            .OrderBy(item => item.DataAvaliacao)
            .Select(item => new BodyEvolutionPointDto(
                item.DataAvaliacao,
                item.PesoKg,
                item.Imc,
                item.PercentualMassaGorda,
                item.MassaGordaKg,
                item.PercentualMassaMagra,
                item.MassaMagraKg,
                item.PercentualAgua,
                item.CinturaCm,
                item.QuadrilCm))
            .ToList();

        return Result<IReadOnlyList<BodyEvolutionPointDto>>.Success(pontos);
    }
}
