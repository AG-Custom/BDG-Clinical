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
using DomainCalculo = AG.CLINICAL.Domain.Services.CalculoEnergetico;

namespace AG.CLINICAL.Application.ClinicalRecords.Nutrition;

public interface IListEnergyCalculationsService
{
    Task<Result<IReadOnlyList<EnergyCalculationDto>>> ExecuteAsync(Guid atendimentoId, CancellationToken cancellationToken = default);
}

public sealed class ListEnergyCalculationsService : IListEnergyCalculationsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly INutritionRecordsRepository _repository;

    public ListEnergyCalculationsService(ICurrentTenantContext tenantContext, INutritionRecordsRepository repository)
    {
        _tenantContext = tenantContext;
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<EnergyCalculationDto>>> ExecuteAsync(
        Guid atendimentoId,
        CancellationToken cancellationToken = default)
    {
        var itens = await _repository.ListVentaByAtendimentoAsync(_tenantContext.EmpresaId, atendimentoId, cancellationToken);
        return Result<IReadOnlyList<EnergyCalculationDto>>.Success(itens.Select(ClinicalRecordMapper.Map).ToList());
    }
}

public interface ICreateEnergyCalculationsService
{
    Task<Result<EnergyCalculationDto>> ExecuteAsync(
        Guid atendimentoId,
        CreateEnergyCalculationRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class CreateEnergyCalculationsService : ICreateEnergyCalculationsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IClinicalEncountersRepository _encountersRepository;
    private readonly IBodyAssessmentsRepository _bodyAssessmentsRepository;
    private readonly INutritionRecordsRepository _nutritionRepository;
    private readonly IMedicalRecordsRepository _medicalRecordsRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateEnergyCalculationsService(
        ICurrentTenantContext tenantContext,
        IClinicalEncountersRepository encountersRepository,
        IBodyAssessmentsRepository bodyAssessmentsRepository,
        INutritionRecordsRepository nutritionRepository,
        IMedicalRecordsRepository medicalRecordsRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _encountersRepository = encountersRepository;
        _bodyAssessmentsRepository = bodyAssessmentsRepository;
        _nutritionRepository = nutritionRepository;
        _medicalRecordsRepository = medicalRecordsRepository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<EnergyCalculationDto>> ExecuteAsync(
        Guid atendimentoId,
        CreateEnergyCalculationRequest request,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;
        var atendimento = await _encountersRepository.GetByIdAndEmpresaIdAsync(atendimentoId, empresaId, cancellationToken);
        if (atendimento is null)
        {
            return Result<EnergyCalculationDto>.Failure("Atendimento não encontrado.");
        }

        var ultima = await _bodyAssessmentsRepository.GetLatestByPacienteAsync(empresaId, atendimento.PacienteId, cancellationToken);
        var peso = request.PesoKg ?? ultima?.PesoKg;
        var altura = request.AlturaCm ?? ultima?.AlturaCm;
        var massaMagra = request.MassaMagraKg ?? ultima?.MassaMagraKg;
        var idade = CalculoCorporal.CalcularIdade(
            atendimento.Paciente?.DataNascimento,
            DateOnly.FromDateTime(DateTime.UtcNow));

        if (peso is null || altura is null || idade is null || atendimento.Paciente?.Sexo is null)
        {
            return Result<EnergyCalculationDto>.Failure("Informe peso, altura, sexo e data de nascimento do paciente.");
        }

        try
        {
            var perfil = ClinicalRecordEnums.Parse<PerfilCalculoEnergetico>(request.Perfil, "Perfil inválido.");
            var protocolo = ClinicalRecordEnums.Parse<ProtocoloGastoEnergetico>(request.Protocolo, "Protocolo inválido.");
            var nivel = ClinicalRecordEnums.Parse<NivelAtividadeFisica>(request.NivelAtividade, "Nível de atividade inválido.");

            if (perfil == PerfilCalculoEnergetico.Atleta
                && protocolo is not (ProtocoloGastoEnergetico.KatchMcArdle or ProtocoloGastoEnergetico.Cunningham))
            {
                return Result<EnergyCalculationDto>.Failure("Perfil atleta utiliza Katch-McArdle ou Cunningham.");
            }

            var tmb = DomainCalculo.CalcularTaxaMetabolicaBasal(
                protocolo,
                atendimento.Paciente.Sexo.Value,
                peso.Value,
                altura.Value,
                idade.Value,
                massaMagra);

            if (tmb is null)
            {
                return Result<EnergyCalculationDto>.Failure("Não foi possível calcular a taxa metabólica com os dados informados.");
            }

            var atividades = (request.Atividades ?? [])
                .Select(item => new AtividadeFisicaDetalhe(item.Nome, item.Mets, item.Minutos))
                .ToList();
            var gastoAtividades = DomainCalculo.CalcularGastoAtividades(peso.Value, atividades);
            var resultado = DomainCalculo.CalcularVenta(
                tmb.Value,
                nivel,
                request.FatorInjuria,
                gastoAtividades,
                peso.Value,
                request.PesoDesejadoKg,
                request.TempoDias);

            var registro = CalculoEnergeticoRegistro.Create(
                empresaId,
                atendimento.Id,
                atendimento.PacienteId,
                atendimento.UnidadeId,
                atendimento.FuncionarioId);

            registro.ApplyResult(
                perfil,
                protocolo,
                nivel,
                request.FatorInjuria,
                peso.Value,
                altura.Value,
                idade.Value,
                massaMagra,
                request.PesoDesejadoKg,
                request.TempoDias,
                request.Atividades is null ? null : ClinicalRecordJson.Serialize(request.Atividades),
                resultado.GastoEnergeticoBasal,
                resultado.GastoEnergeticoTotal,
                resultado.AjusteCaloricoDiario,
                resultado.MetaCaloricaDiaria);

            await _nutritionRepository.AddVentaAsync(registro, cancellationToken);
            await _medicalRecordsRepository.AddEventoAsync(
                EventoClinico.Create(
                    empresaId,
                    atendimento.PacienteId,
                    atendimento.Id,
                    TipoEventoClinico.VentaCalculada,
                    "VENTA calculada",
                    $"Meta {resultado.MetaCaloricaDiaria} kcal",
                    nameof(CalculoEnergeticoRegistro),
                    registro.Id,
                    atendimento.FuncionarioId,
                    atendimento.UnidadeId),
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _auditLogsService.RegisterEntityChangeAsync(
                empresaId,
                _tenantContext.UsuarioId,
                nameof(CalculoEnergeticoRegistro),
                registro.Id,
                AcaoAuditoria.Criar,
                dadosNovos: ClinicalRecordAudit.Summary(new { resultado.MetaCaloricaDiaria }),
                cancellationToken: cancellationToken);

            return Result<EnergyCalculationDto>.Success(ClinicalRecordMapper.Map(registro));
        }
        catch (DomainException exception)
        {
            return Result<EnergyCalculationDto>.Failure(exception.Message);
        }
    }
}

public interface IListPocketRulesService
{
    Task<Result<IReadOnlyList<PocketRuleDto>>> ExecuteAsync(Guid atendimentoId, CancellationToken cancellationToken = default);
}

public sealed class ListPocketRulesService : IListPocketRulesService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly INutritionRecordsRepository _repository;

    public ListPocketRulesService(ICurrentTenantContext tenantContext, INutritionRecordsRepository repository)
    {
        _tenantContext = tenantContext;
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<PocketRuleDto>>> ExecuteAsync(
        Guid atendimentoId,
        CancellationToken cancellationToken = default)
    {
        var itens = await _repository.ListRegraBolsoByAtendimentoAsync(_tenantContext.EmpresaId, atendimentoId, cancellationToken);
        return Result<IReadOnlyList<PocketRuleDto>>.Success(itens.Select(ClinicalRecordMapper.Map).ToList());
    }
}

public interface ICreatePocketRulesService
{
    Task<Result<PocketRuleDto>> ExecuteAsync(
        Guid atendimentoId,
        CreatePocketRuleRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class CreatePocketRulesService : ICreatePocketRulesService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IClinicalEncountersRepository _encountersRepository;
    private readonly IBodyAssessmentsRepository _bodyAssessmentsRepository;
    private readonly INutritionRecordsRepository _nutritionRepository;
    private readonly IMedicalRecordsRepository _medicalRecordsRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePocketRulesService(
        ICurrentTenantContext tenantContext,
        IClinicalEncountersRepository encountersRepository,
        IBodyAssessmentsRepository bodyAssessmentsRepository,
        INutritionRecordsRepository nutritionRepository,
        IMedicalRecordsRepository medicalRecordsRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _encountersRepository = encountersRepository;
        _bodyAssessmentsRepository = bodyAssessmentsRepository;
        _nutritionRepository = nutritionRepository;
        _medicalRecordsRepository = medicalRecordsRepository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PocketRuleDto>> ExecuteAsync(
        Guid atendimentoId,
        CreatePocketRuleRequest request,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;
        var atendimento = await _encountersRepository.GetByIdAndEmpresaIdAsync(atendimentoId, empresaId, cancellationToken);
        if (atendimento is null)
        {
            return Result<PocketRuleDto>.Failure("Atendimento não encontrado.");
        }

        var ultima = await _bodyAssessmentsRepository.GetLatestByPacienteAsync(empresaId, atendimento.PacienteId, cancellationToken);
        var peso = request.PesoKg ?? ultima?.PesoKg;
        var gasto = request.GastoEnergeticoTotal;
        if (gasto is null)
        {
            var vendas = await _nutritionRepository.ListVentaByAtendimentoAsync(empresaId, atendimentoId, cancellationToken);
            gasto = vendas.FirstOrDefault()?.GastoEnergeticoTotal;
        }

        if (peso is null || gasto is null)
        {
            return Result<PocketRuleDto>.Failure("Informe peso e gasto energético (ou calcule a VENTA antes).");
        }

        try
        {
            var objetivo = ClinicalRecordEnums.Parse<ObjetivoRegraBolso>(request.Objetivo, "Objetivo inválido.");
            var resultado = DomainCalculo.CalcularRegraBolso(objetivo, peso.Value, gasto.Value);
            var registro = RegraBolsoRegistro.Create(
                empresaId,
                atendimento.Id,
                atendimento.PacienteId,
                atendimento.UnidadeId,
                atendimento.FuncionarioId);

            registro.ApplyResult(
                objetivo,
                peso.Value,
                gasto.Value,
                resultado.Calorias,
                resultado.ProteinasG,
                resultado.CarboidratosG,
                resultado.GordurasG);

            await _nutritionRepository.AddRegraBolsoAsync(registro, cancellationToken);
            await _medicalRecordsRepository.AddEventoAsync(
                EventoClinico.Create(
                    empresaId,
                    atendimento.PacienteId,
                    atendimento.Id,
                    TipoEventoClinico.RegraBolsoCalculada,
                    "Regra de bolso calculada",
                    $"{objetivo} · {resultado.Calorias} kcal",
                    nameof(RegraBolsoRegistro),
                    registro.Id,
                    atendimento.FuncionarioId,
                    atendimento.UnidadeId),
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _auditLogsService.RegisterEntityChangeAsync(
                empresaId,
                _tenantContext.UsuarioId,
                nameof(RegraBolsoRegistro),
                registro.Id,
                AcaoAuditoria.Criar,
                dadosNovos: ClinicalRecordAudit.Summary(new { resultado.Calorias, objetivo }),
                cancellationToken: cancellationToken);

            return Result<PocketRuleDto>.Success(ClinicalRecordMapper.Map(registro));
        }
        catch (DomainException exception)
        {
            return Result<PocketRuleDto>.Failure(exception.Message);
        }
    }
}
