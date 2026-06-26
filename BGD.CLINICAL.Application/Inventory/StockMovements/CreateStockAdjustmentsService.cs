using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Core.Abstractions;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Application.Inventory.Dtos;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using BGD.CLINICAL.Domain.Exceptions;

namespace BGD.CLINICAL.Application.Inventory.StockMovements;

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
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateStockAdjustmentsService(
        ICurrentTenantContext tenantContext,
        IUnitsRepository unitsRepository,
        IProductsRepository productsRepository,
        IStockBalancesRepository stockBalancesRepository,
        IUsersRepository usersRepository,
        IStockMovementsRepository stockMovementsRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _unitsRepository = unitsRepository;
        _productsRepository = productsRepository;
        _stockBalancesRepository = stockBalancesRepository;
        _usersRepository = usersRepository;
        _stockMovementsRepository = stockMovementsRepository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<StockMovementDto>> ExecuteAsync(
        CreateManualStockMovementRequest request,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;

        var validation = await StockMovementRequestValidator.ValidateManualAsync(
            empresaId,
            _tenantContext.UsuarioId,
            request,
            requireAvailableBalance: false,
            _unitsRepository,
            _productsRepository,
            _stockBalancesRepository,
            _usersRepository,
            cancellationToken);

        if (validation.IsFailure)
        {
            return Result<StockMovementDto>.Failure(validation.Error!);
        }

        try
        {
            var data = validation.Value!;
            var movimentacao = MovimentacaoEstoque.CreateAjusteManual(
                empresaId,
                data.UnidadeId,
                data.ProdutoId,
                data.Quantidade,
                data.Data,
                data.FuncionarioId,
                data.Observacao);

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
