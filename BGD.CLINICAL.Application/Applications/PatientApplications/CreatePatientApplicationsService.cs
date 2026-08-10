using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Applications.Abstractions;
using BGD.CLINICAL.Application.Applications.Dtos;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Core.Abstractions;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Application.Packages.Abstractions;
using BGD.CLINICAL.Application.Patients.Abstractions;
using BGD.CLINICAL.Application.Inventory.StockMovements;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using BGD.CLINICAL.Domain.Exceptions;

namespace BGD.CLINICAL.Application.Applications.PatientApplications;

public interface ICreatePatientApplicationsService
{
    Task<Result<CreatePatientApplicationsResult>> ExecuteAsync(
        CreatePatientApplicationRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class CreatePatientApplicationsService : ICreatePatientApplicationsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IPatientApplicationsRepository _patientApplicationsRepository;
    private readonly IPatientPurchasesRepository _patientPurchasesRepository;
    private readonly IPatientsRepository _patientsRepository;
    private readonly IProductsRepository _productsRepository;
    private readonly IProceduresRepository _proceduresRepository;
    private readonly IUnitsRepository _unitsRepository;
    private readonly IEmployeesRepository _employeesRepository;
    private readonly ISymptomsRepository _symptomsRepository;
    private readonly IStockBalancesRepository _stockBalancesRepository;
    private readonly IStockMovementsRepository _stockMovementsRepository;
    private readonly IMedicationLotStockService _medicationLotStockService;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePatientApplicationsService(
        ICurrentTenantContext tenantContext,
        IPatientApplicationsRepository patientApplicationsRepository,
        IPatientPurchasesRepository patientPurchasesRepository,
        IPatientsRepository patientsRepository,
        IProductsRepository productsRepository,
        IProceduresRepository proceduresRepository,
        IUnitsRepository unitsRepository,
        IEmployeesRepository employeesRepository,
        ISymptomsRepository symptomsRepository,
        IStockBalancesRepository stockBalancesRepository,
        IStockMovementsRepository stockMovementsRepository,
        IMedicationLotStockService medicationLotStockService,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _patientApplicationsRepository = patientApplicationsRepository;
        _patientPurchasesRepository = patientPurchasesRepository;
        _patientsRepository = patientsRepository;
        _productsRepository = productsRepository;
        _proceduresRepository = proceduresRepository;
        _unitsRepository = unitsRepository;
        _employeesRepository = employeesRepository;
        _symptomsRepository = symptomsRepository;
        _stockBalancesRepository = stockBalancesRepository;
        _stockMovementsRepository = stockMovementsRepository;
        _medicationLotStockService = medicationLotStockService;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CreatePatientApplicationsResult>> ExecuteAsync(
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
            _patientPurchasesRepository,
            _medicationLotStockService,
            cancellationToken);

        if (validation.IsFailure)
        {
            return Result<CreatePatientApplicationsResult>.Failure(validation.Error!);
        }

        try
        {
            var data = validation.Value!;
            CompraPaciente? compra = null;

            if (data.CompraPacienteId.HasValue)
            {
                compra = await _patientPurchasesRepository.GetByIdAndEmpresaIdWithDetailsAsync(
                    data.CompraPacienteId.Value,
                    empresaId,
                    cancellationToken);

                if (compra is null)
                {
                    return Result<CreatePatientApplicationsResult>.Failure("Compra de pacote não encontrada.");
                }
            }

            var aplicacoesCriadas = new List<AplicacaoPaciente>();
            var todasMovimentacoes = new List<MovimentacaoEstoque>();
            var aplicarPeso = data.Procedimentos.Count == 1 ? data.Peso : null;

            foreach (var procedimentoData in data.Procedimentos)
            {
                if (compra is not null)
                {
                    compra.EnsurePodeAplicar(
                        data.PacienteId,
                        procedimentoData.ProdutoId,
                        procedimentoData.QuantidadeUtilizada);
                }

                var aplicacao = AplicacaoPaciente.CreateRealizada(
                    empresaId,
                    data.PacienteId,
                    data.CompraPacienteId,
                    procedimentoData.ProdutoId,
                    procedimentoData.ProcedimentoId,
                    data.AplicadorId,
                    data.UnidadeId,
                    data.DataAplicacao,
                    procedimentoData.QuantidadeUtilizada,
                    aplicarPeso,
                    data.Observacao);

                foreach (var sintomaId in data.SintomaIds)
                {
                    aplicacao.Sintomas.Add(new AplicacaoSintoma(aplicacao.Id, sintomaId));
                }

                var stockLines = procedimentoData.StockLines.Where(line => line.ControlaEstoque).ToList();
                var produtosEstoque = await _productsRepository.GetActiveByIdsAndEmpresaIdAsync(
                    empresaId,
                    stockLines.Select(line => line.ProdutoId).Distinct().ToList(),
                    cancellationToken);
                var produtosPorId = produtosEstoque.ToDictionary(produto => produto.Id);

                foreach (var line in stockLines)
                {
                    if (!produtosPorId.TryGetValue(line.ProdutoId, out var produtoLinha))
                    {
                        return Result<CreatePatientApplicationsResult>.Failure("Produto de estoque não encontrado.");
                    }

                    var isProdutoAplicado = procedimentoData.ProdutoId.HasValue
                        && line.ProdutoId == procedimentoData.ProdutoId.Value;

                    if (isProdutoAplicado
                        && _medicationLotStockService.RequiresLot(produtoLinha)
                        && procedimentoData.LoteProdutoId.HasValue)
                    {
                        var alocacao = await _medicationLotStockService.AllocateFromLotAsync(
                            empresaId,
                            data.UnidadeId,
                            produtoLinha,
                            procedimentoData.LoteProdutoId.Value,
                            line.Quantidade,
                            cancellationToken);

                        var movimentacao = MovimentacaoEstoque.CreateSaidaFromAplicacao(
                            empresaId,
                            data.UnidadeId,
                            line.ProdutoId,
                            aplicacao.Id,
                            data.AplicadorId,
                            alocacao.Quantidade,
                            data.DataAplicacao);
                        movimentacao.AssignLote(alocacao.LoteProdutoId);
                        todasMovimentacoes.Add(movimentacao);
                    }
                    else
                    {
                        todasMovimentacoes.Add(MovimentacaoEstoque.CreateSaidaFromAplicacao(
                            empresaId,
                            data.UnidadeId,
                            line.ProdutoId,
                            aplicacao.Id,
                            data.AplicadorId,
                            line.Quantidade,
                            data.DataAplicacao));
                    }
                }

                await _patientApplicationsRepository.AddAsync(aplicacao, cancellationToken);

                if (compra is not null)
                {
                    compra.Aplicacoes.Add(aplicacao);
                }

                aplicacoesCriadas.Add(aplicacao);
            }

            if (compra is not null)
            {
                compra.CompleteIfExhausted();
                _patientPurchasesRepository.Update(compra);
            }

            if (todasMovimentacoes.Count > 0)
            {
                await _stockMovementsRepository.AddRangeAsync(todasMovimentacoes, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dtos = new List<PatientApplicationDto>();

            foreach (var aplicacao in aplicacoesCriadas)
            {
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

                dtos.Add(PatientApplicationsMapper.Map(persisted ?? aplicacao));
            }

            foreach (var movimentacao in todasMovimentacoes)
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

            return Result<CreatePatientApplicationsResult>.Success(new CreatePatientApplicationsResult(dtos));
        }
        catch (DomainException exception)
        {
            return Result<CreatePatientApplicationsResult>.Failure(exception.Message);
        }
    }
}
