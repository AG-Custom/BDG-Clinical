using AG.CLINICAL.Application.Abstractions.Persistence;
using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Core.Abstractions;
using AG.CLINICAL.Application.Identity.Abstractions;
using AG.CLINICAL.Application.Inventory.Abstractions;
using AG.CLINICAL.Application.Inventory.Dtos;
using AG.CLINICAL.Domain.Entities;
using AG.CLINICAL.Domain.Enums;
using AG.CLINICAL.Domain.Exceptions;

namespace AG.CLINICAL.Application.Inventory.StockMovements;

public interface ICreateStockAdjustmentsService
{
    Task<Result<IReadOnlyList<StockMovementDto>>> ExecuteAsync(
        CreateManualStockMovementRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class CreateStockAdjustmentsService : ICreateStockAdjustmentsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IUnitsRepository _unitsRepository;
    private readonly IProductsRepository _productsRepository;
    private readonly IStockBalancesRepository _stockBalancesRepository;
    private readonly IUsersRepository _usersRepository;
    private readonly IStockMovementsRepository _stockMovementsRepository;
    private readonly IMedicationLotStockService _medicationLotStockService;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateStockAdjustmentsService(
        ICurrentTenantContext tenantContext,
        IUnitsRepository unitsRepository,
        IProductsRepository productsRepository,
        IStockBalancesRepository stockBalancesRepository,
        IUsersRepository usersRepository,
        IStockMovementsRepository stockMovementsRepository,
        IMedicationLotStockService medicationLotStockService,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _unitsRepository = unitsRepository;
        _productsRepository = productsRepository;
        _stockBalancesRepository = stockBalancesRepository;
        _usersRepository = usersRepository;
        _stockMovementsRepository = stockMovementsRepository;
        _medicationLotStockService = medicationLotStockService;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<StockMovementDto>>> ExecuteAsync(
        CreateManualStockMovementRequest request,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;

        try
        {
            var produto = await _productsRepository.GetByIdAndEmpresaIdAsync(
                request.ProdutoId,
                empresaId,
                cancellationToken);

            if (produto is null || !produto.Ativo)
            {
                return Result<IReadOnlyList<StockMovementDto>>.Failure("Produto não encontrado ou inativo.");
            }

            var requiresLot = _medicationLotStockService.RequiresLot(produto);

            var validation = await StockMovementRequestValidator.ValidateManualAsync(
                empresaId,
                _tenantContext.UsuarioId,
                request,
                requireAvailableBalance: false,
                requiresLot,
                _unitsRepository,
                _productsRepository,
                _stockBalancesRepository,
                _usersRepository,
                cancellationToken);

            if (validation.IsFailure)
            {
                return Result<IReadOnlyList<StockMovementDto>>.Failure(validation.Error!);
            }

            var data = validation.Value!;
            var movimentacoes = new List<MovimentacaoEstoque>();

            if (requiresLot)
            {
                var lotes = request.Lotes is { Count: > 0 }
                    ? request.Lotes
                    :
                    [
                        new CreateManualStockMovementLotRequest(
                            request.LoteCodigo!,
                            request.QuantidadeEmbalagem!.Value,
                            request.DataValidade!.Value)
                    ];

                foreach (var lote in lotes)
                {
                    var entry = await _medicationLotStockService.ResolveEntryAsync(
                        empresaId,
                        data.UnidadeId,
                        produto,
                        lote.QuantidadeEmbalagem,
                        lote.LoteCodigo,
                        lote.DataValidade,
                        cancellationToken);

                    var movimentacao = CreateMovement(
                        empresaId,
                        data,
                        entry.QuantidadeEstoque,
                        request.ValorUnitario,
                        produto.FatorEmbalagemParaEstoque);
                    movimentacao.AssignLote(entry.Lote.Id, lote.QuantidadeEmbalagem);
                    movimentacoes.Add(movimentacao);
                }
            }
            else
            {
                movimentacoes.Add(CreateMovement(
                    empresaId,
                    data,
                    request.Quantidade!.Value,
                    request.ValorUnitario,
                    fatorEmbalagemParaEstoque: null));
            }

            await _stockMovementsRepository.AddRangeAsync(movimentacoes, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var resultado = new List<StockMovementDto>();
            foreach (var movimentacao in movimentacoes)
            {
                await _auditLogsService.RegisterEntityChangeAsync(
                    empresaId,
                    _tenantContext.UsuarioId,
                    nameof(MovimentacaoEstoque),
                    movimentacao.Id,
                    AcaoAuditoria.GerarMovimentacao,
                    dadosNovos: StockMovementsAuditSerializer.Serialize(movimentacao),
                    cancellationToken: cancellationToken);

                var persisted = await _stockMovementsRepository.GetByIdAndEmpresaIdWithDetailsAsync(
                    movimentacao.Id,
                    empresaId,
                    cancellationToken);
                resultado.Add(StockMovementsMapper.Map(persisted ?? movimentacao));
            }

            return Result<IReadOnlyList<StockMovementDto>>.Success(resultado);
        }
        catch (DomainException exception)
        {
            return Result<IReadOnlyList<StockMovementDto>>.Failure(exception.Message);
        }
    }

    private static MovimentacaoEstoque CreateMovement(
        Guid empresaId,
        ValidatedManualStockMovementData data,
        decimal quantidade,
        decimal? valorUnitario,
        decimal? fatorEmbalagemParaEstoque)
    {
        var movimentacao = MovimentacaoEstoque.CreateAjusteManual(
            empresaId,
            data.UnidadeId,
            data.ProdutoId,
            quantidade,
            data.Data,
            data.FuncionarioId,
            data.Observacao);

        if (valorUnitario.HasValue)
        {
            var valorPorUnidadeEstoque = ProductStockValuation.ResolveValorPorUnidadeEstoque(
                valorUnitario.Value,
                fatorEmbalagemParaEstoque);
            movimentacao.AssignValorUnitario(valorPorUnidadeEstoque);
        }

        return movimentacao;
    }
}
