using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Inventory.Abstractions;
using AG.CLINICAL.Application.Inventory.Dtos;

namespace AG.CLINICAL.Application.Inventory.StockBalances;

public interface IListStockLotBalancesService
{
    Task<Result<IReadOnlyList<StockLotBalanceDto>>> ExecuteAsync(
        Guid? unidadeId,
        Guid? produtoId,
        CancellationToken cancellationToken = default);
}

public sealed class ListStockLotBalancesService : IListStockLotBalancesService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IStockBalancesRepository _stockBalancesRepository;

    public ListStockLotBalancesService(
        ICurrentTenantContext tenantContext,
        IStockBalancesRepository stockBalancesRepository)
    {
        _tenantContext = tenantContext;
        _stockBalancesRepository = stockBalancesRepository;
    }

    public async Task<Result<IReadOnlyList<StockLotBalanceDto>>> ExecuteAsync(
        Guid? unidadeId,
        Guid? produtoId,
        CancellationToken cancellationToken = default)
    {
        var rows = await _stockBalancesRepository.ListLotBalancesAsync(
            _tenantContext.EmpresaId,
            unidadeId,
            produtoId,
            cancellationToken);

        var dtos = rows
            .Select(row => new StockLotBalanceDto(
                row.LoteProdutoId,
                row.UnidadeId,
                row.UnidadeNome,
                row.ProdutoId,
                row.ProdutoNome,
                row.Codigo,
                row.DataValidade,
                row.SaldoAtual,
                row.UnidadeMedidaSigla,
                row.FatorEmbalagemParaEstoque,
                row.FatorEmbalagemParaEstoque is > 0
                    ? Math.Round(row.SaldoAtual / row.FatorEmbalagemParaEstoque.Value, 4)
                    : null))
            .ToList();

        return Result<IReadOnlyList<StockLotBalanceDto>>.Success(dtos);
    }
}
