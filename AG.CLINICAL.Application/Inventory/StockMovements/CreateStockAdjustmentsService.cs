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
    Task<Result<StockMovementDto>> ExecuteAsync(
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

    public async Task<Result<StockMovementDto>> ExecuteAsync(
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
                return Result<StockMovementDto>.Failure("Produto não encontrado ou inativo.");
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
                return Result<StockMovementDto>.Failure(validation.Error!);
            }

            var data = validation.Value!;
            decimal quantidade;
            Guid? loteId = null;
            decimal? quantidadeEmbalagem = null;

            if (requiresLot)
            {
                var entry = await _medicationLotStockService.ResolveEntryAsync(
                    empresaId,
                    data.UnidadeId,
                    produto,
                    request.QuantidadeEmbalagem!.Value,
                    request.LoteCodigo!,
                    request.DataValidade!.Value,
                    cancellationToken);

                quantidade = entry.QuantidadeEstoque;
                loteId = entry.Lote.Id;
                quantidadeEmbalagem = request.QuantidadeEmbalagem;
            }
            else
            {
                quantidade = request.Quantidade!.Value;
            }

            var movimentacao = MovimentacaoEstoque.CreateAjusteManual(
                empresaId,
                data.UnidadeId,
                data.ProdutoId,
                quantidade,
                data.Data,
                data.FuncionarioId,
                data.Observacao);

            if (request.ValorUnitario.HasValue)
            {
                var valorPorUnidadeEstoque = ProductStockValuation.ResolveValorPorUnidadeEstoque(
                    request.ValorUnitario.Value,
                    requiresLot ? produto.FatorEmbalagemParaEstoque : null);
                movimentacao.AssignValorUnitario(valorPorUnidadeEstoque);
            }

            if (loteId.HasValue)
            {
                movimentacao.AssignLote(loteId.Value, quantidadeEmbalagem);
            }

            await _stockMovementsRepository.AddAsync(movimentacao, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

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

            return Result<StockMovementDto>.Success(
                StockMovementsMapper.Map(persisted ?? movimentacao));
        }
        catch (DomainException exception)
        {
            return Result<StockMovementDto>.Failure(exception.Message);
        }
    }
}
