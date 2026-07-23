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
    Task<Result<PatientApplicationDto>> ExecuteAsync(
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
            _patientPurchasesRepository,
            cancellationToken);

        if (validation.IsFailure)
        {
            return Result<PatientApplicationDto>.Failure(validation.Error!);
        }

        try
        {
            var data = validation.Value!;
            var compra = await _patientPurchasesRepository.GetByIdAndEmpresaIdWithDetailsAsync(
                data.CompraPacienteId,
                empresaId,
                cancellationToken);

            if (compra is null)
            {
                return Result<PatientApplicationDto>.Failure("Compra de pacote não encontrada.");
            }

            compra.EnsurePodeAplicar(data.PacienteId, data.ProdutoId, data.QuantidadeUtilizada);

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

            var stockLines = data.StockLines.Where(line => line.ControlaEstoque).ToList();
            var produtosEstoque = await _productsRepository.GetActiveByIdsAndEmpresaIdAsync(
                empresaId,
                stockLines.Select(line => line.ProdutoId).Distinct().ToList(),
                cancellationToken);
            var produtosPorId = produtosEstoque.ToDictionary(produto => produto.Id);

            var movimentacoes = new List<MovimentacaoEstoque>();

            foreach (var line in stockLines)
            {
                if (!produtosPorId.TryGetValue(line.ProdutoId, out var produtoLinha))
                {
                    return Result<PatientApplicationDto>.Failure("Produto de estoque não encontrado.");
                }

                if (_medicationLotStockService.RequiresLot(produtoLinha))
                {
                    var alocacoes = await _medicationLotStockService.AllocateFefoAsync(
                        empresaId,
                        data.UnidadeId,
                        produtoLinha,
                        line.Quantidade,
                        cancellationToken);

                    foreach (var alocacao in alocacoes)
                    {
                        var movimentacao = MovimentacaoEstoque.CreateSaidaFromAplicacao(
                            empresaId,
                            data.UnidadeId,
                            line.ProdutoId,
                            aplicacao.Id,
                            data.AplicadorId,
                            alocacao.Quantidade,
                            data.DataAplicacao);
                        movimentacao.AssignLote(alocacao.LoteProdutoId);
                        movimentacoes.Add(movimentacao);
                    }
                }
                else
                {
                    movimentacoes.Add(MovimentacaoEstoque.CreateSaidaFromAplicacao(
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
            if (movimentacoes.Count > 0)
            {
                await _stockMovementsRepository.AddRangeAsync(movimentacoes, cancellationToken);
            }

            compra.Aplicacoes.Add(aplicacao);
            compra.CompleteIfExhausted();
            _patientPurchasesRepository.Update(compra);

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
