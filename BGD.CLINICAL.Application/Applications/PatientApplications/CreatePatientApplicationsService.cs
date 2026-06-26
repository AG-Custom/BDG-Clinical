using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Applications.Abstractions;
using BGD.CLINICAL.Application.Applications.Dtos;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Application.Patients.Abstractions;
using BGD.CLINICAL.Application.Core.Abstractions;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using BGD.CLINICAL.Domain.Exceptions;

namespace BGD.CLINICAL.Application.Applications.PatientApplications;

public interface ICreatePatientApplicationsService
{
    Task<Result<PatientApplicationDto>> ExecuteAsync(
        CreatePatientApplicationRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class CreatePatientApplicationsService : ICreatePatientApplicationsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IPatientApplicationsRepository _patientApplicationsRepository;
    private readonly IPatientsRepository _patientsRepository;
    private readonly IProductsRepository _productsRepository;
    private readonly IProceduresRepository _proceduresRepository;
    private readonly IUnitsRepository _unitsRepository;
    private readonly IEmployeesRepository _employeesRepository;
    private readonly ISymptomsRepository _symptomsRepository;
    private readonly IStockBalancesRepository _stockBalancesRepository;
    private readonly IStockMovementsRepository _stockMovementsRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePatientApplicationsService(
        ICurrentTenantContext tenantContext,
        IPatientApplicationsRepository patientApplicationsRepository,
        IPatientsRepository patientsRepository,
        IProductsRepository productsRepository,
        IProceduresRepository proceduresRepository,
        IUnitsRepository unitsRepository,
        IEmployeesRepository employeesRepository,
        ISymptomsRepository symptomsRepository,
        IStockBalancesRepository stockBalancesRepository,
        IStockMovementsRepository stockMovementsRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _patientApplicationsRepository = patientApplicationsRepository;
        _patientsRepository = patientsRepository;
        _productsRepository = productsRepository;
        _proceduresRepository = proceduresRepository;
        _unitsRepository = unitsRepository;
        _employeesRepository = employeesRepository;
        _symptomsRepository = symptomsRepository;
        _stockBalancesRepository = stockBalancesRepository;
        _stockMovementsRepository = stockMovementsRepository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PatientApplicationDto>> ExecuteAsync(
        CreatePatientApplicationRequest request,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;

        var validation = await PatientApplicationRequestValidator.ValidateCreateAsync(
            empresaId,
            request,
            _patientsRepository,
            _productsRepository,
            _proceduresRepository,
            _unitsRepository,
            _employeesRepository,
            _symptomsRepository,
            _stockBalancesRepository,
            _patientApplicationsRepository,
            cancellationToken);

        if (validation.IsFailure)
        {
            return Result<PatientApplicationDto>.Failure(validation.Error!);
        }

        try
        {
            var data = validation.Value!;
            var aplicacao = AplicacaoPaciente.CreateRealizada(
                empresaId,
                data.PacienteId,
                data.CompraPacienteId,
                data.ProdutoId,
                data.ProcedimentoId,
                data.AplicadorId,
                data.UnidadeId,
                data.DataAplicacao,
                data.QuantidadeUtilizada,
                data.Peso,
                data.Observacao);

            foreach (var sintomaId in data.SintomaIds)
            {
                aplicacao.Sintomas.Add(new AplicacaoSintoma(aplicacao.Id, sintomaId));
            }

            var movimentacoes = data.StockLines
                .Where(line => line.ControlaEstoque)
                .Select(line => MovimentacaoEstoque.CreateSaidaFromAplicacao(
                    empresaId,
                    data.UnidadeId,
                    line.ProdutoId,
                    aplicacao.Id,
                    data.AplicadorId,
                    line.Quantidade,
                    data.DataAplicacao))
                .ToList();

            await _patientApplicationsRepository.AddAsync(aplicacao, cancellationToken);
            if (movimentacoes.Count > 0)
            {
                await _stockMovementsRepository.AddRangeAsync(movimentacoes, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var persisted = await _patientApplicationsRepository.GetByIdAndEmpresaIdWithDetailsAsync(
                aplicacao.Id,
                empresaId,
                cancellationToken);

            await _auditLogsService.RegisterEntityChangeAsync(
                empresaId,
                _tenantContext.UsuarioId,
                nameof(AplicacaoPaciente),
                aplicacao.Id,
                AcaoAuditoria.Criar,
                dadosNovos: PatientApplicationsAuditSerializer.Serialize(persisted ?? aplicacao),
                cancellationToken: cancellationToken);

            foreach (var movimentacao in movimentacoes)
            {
                await _auditLogsService.RegisterEntityChangeAsync(
                    empresaId,
                    _tenantContext.UsuarioId,
                    nameof(MovimentacaoEstoque),
                    movimentacao.Id,
                    AcaoAuditoria.GerarMovimentacao,
                    dadosNovos: PatientApplicationsAuditSerializer.Serialize(movimentacao),
                    cancellationToken: cancellationToken);
            }

            return Result<PatientApplicationDto>.Success(
                PatientApplicationsMapper.Map(persisted ?? aplicacao));
        }
        catch (DomainException exception)
        {
            return Result<PatientApplicationDto>.Failure(exception.Message);
        }
    }
}
