using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Application.Inventory.Dtos;

namespace BGD.CLINICAL.Application.Inventory.StockMovements;

public interface IListStockMovementsService
{
    Task<Result<IReadOnlyList<StockMovementDto>>> ExecuteAsync(
        Guid? unidadeId = null,
        Guid? produtoId = null,
        string? tipo = null,
        DateTime? dataInicio = null,
        DateTime? dataFim = null,
        int? limit = null,
        CancellationToken cancellationToken = default);
}

public sealed class ListStockMovementsService : IListStockMovementsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IStockMovementsRepository _stockMovementsRepository;

    public ListStockMovementsService(
        ICurrentTenantContext tenantContext,
        IStockMovementsRepository stockMovementsRepository)
    {
        _tenantContext = tenantContext;
        _stockMovementsRepository = stockMovementsRepository;
    }

    public async Task<Result<IReadOnlyList<StockMovementDto>>> ExecuteAsync(
        Guid? unidadeId = null,
        Guid? produtoId = null,
        string? tipo = null,
        DateTime? dataInicio = null,
        DateTime? dataFim = null,
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        var tipoResult = StockMovementValidation.ParseTipo(tipo);
        if (tipoResult.IsFailure)
        {
            return Result<IReadOnlyList<StockMovementDto>>.Failure(tipoResult.Error!);
        }

        var dateRangeResult = StockMovementValidation.ValidateDateRange(dataInicio, dataFim);
        if (dateRangeResult.IsFailure)
        {
            return Result<IReadOnlyList<StockMovementDto>>.Failure(dateRangeResult.Error!);
        }

        var limitResult = StockMovementValidation.ValidateLimit(limit);
        if (limitResult.IsFailure)
        {
            return Result<IReadOnlyList<StockMovementDto>>.Failure(limitResult.Error!);
        }

        var (validatedDataInicio, validatedDataFim) = dateRangeResult.Value!;

        var movimentacoes = await _stockMovementsRepository.ListByEmpresaIdAsync(
            _tenantContext.EmpresaId,
            unidadeId,
            produtoId,
            tipoResult.Value,
            validatedDataInicio,
            validatedDataFim,
            limitResult.Value!,
            cancellationToken);

        return Result<IReadOnlyList<StockMovementDto>>.Success(StockMovementsMapper.Map(movimentacoes));
    }
}
