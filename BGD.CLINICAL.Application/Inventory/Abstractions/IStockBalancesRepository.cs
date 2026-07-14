namespace BGD.CLINICAL.Application.Inventory.Abstractions;

public sealed record StockBalanceRow(
    Guid UnidadeId,
    string UnidadeNome,
    Guid ProdutoId,
    string ProdutoNome,
    string UnidadeMedidaSigla,
    decimal EstoqueMinimo,
    decimal SaldoAtual,
    decimal? ValorUnitario,
    IReadOnlyList<string> OrigensEntrada);

public interface IStockBalancesRepository
{
    Task<IReadOnlyList<StockBalanceRow>> ListByEmpresaIdAsync(
        Guid empresaId,
        Guid? unidadeId,
        Guid? produtoId,
        bool? abaixoDoMinimo,
        string? search,
        int? limit,
        CancellationToken cancellationToken = default);

    Task<decimal> GetSaldoByUnidadeAndProdutoAsync(
        Guid empresaId,
        Guid unidadeId,
        Guid produtoId,
        CancellationToken cancellationToken = default);
}
