using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Application.Inventory.Dtos;
using BGD.CLINICAL.Application.Inventory.MeasurementUnits;

namespace BGD.CLINICAL.Application.Inventory.StockBalances;

public interface IListStockBalancesService
{
    Task<Result<IReadOnlyList<StockBalanceDto>>> ExecuteAsync(
        Guid? unidadeId = null,
        Guid? produtoId = null,
        bool? abaixoDoMinimo = null,
        string? search = null,
        int? limit = null,
        CancellationToken cancellationToken = default);
}

public sealed class ListStockBalancesService : IListStockBalancesService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IStockBalancesRepository _stockBalancesRepository;

    public ListStockBalancesService(
        ICurrentTenantContext tenantContext,
        IStockBalancesRepository stockBalancesRepository)
    {
        _tenantContext = tenantContext;
        _stockBalancesRepository = stockBalancesRepository;
    }

    public async Task<Result<IReadOnlyList<StockBalanceDto>>> ExecuteAsync(
        Guid? unidadeId = null,
        Guid? produtoId = null,
        bool? abaixoDoMinimo = null,
        string? search = null,
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        string? normalizedSearch = null;
        int? effectiveLimit = null;

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchResult = MeasurementUnitValidation.ValidateSearch(search, limit);
            if (searchResult.IsFailure)
            {
                return Result<IReadOnlyList<StockBalanceDto>>.Failure(searchResult.Error!);
            }

            (normalizedSearch, var validatedLimit) = searchResult.Value!;
            effectiveLimit = validatedLimit;
        }
        else if (limit.HasValue)
        {
            var limitResult = MeasurementUnitValidation.ValidateLimit(limit, MeasurementUnitSearchOptions.DefaultLimit);
            if (limitResult.IsFailure)
            {
                return Result<IReadOnlyList<StockBalanceDto>>.Failure(limitResult.Error!);
            }

            effectiveLimit = limitResult.Value;
        }

        var rows = await _stockBalancesRepository.ListByEmpresaIdAsync(
            _tenantContext.EmpresaId,
            unidadeId,
            produtoId,
            abaixoDoMinimo,
            normalizedSearch,
            effectiveLimit,
            cancellationToken);

        var balances = rows
            .Select(row => new StockBalanceDto(
                row.UnidadeId,
                row.UnidadeNome,
                row.ProdutoId,
                row.ProdutoNome,
                row.UnidadeMedidaSigla,
                row.EstoqueMinimo,
                row.SaldoAtual,
                row.ValorUnitario,
                Math.Round(row.SaldoAtual * (row.ValorUnitario ?? 0), 2, MidpointRounding.AwayFromZero),
                row.SaldoAtual < row.EstoqueMinimo))
            .ToList();

        return Result<IReadOnlyList<StockBalanceDto>>.Success(balances);
    }
}
