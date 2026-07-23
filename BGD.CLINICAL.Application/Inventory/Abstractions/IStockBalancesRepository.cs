namespace BGD.CLINICAL.Application.Inventory.Abstractions;

public sealed record StockBalanceRow(
    Guid UnidadeId,
    string UnidadeNome,
    Guid ProdutoId,
    string ProdutoNome,
    string UnidadeMedidaSigla,
    decimal EstoqueMinimo,
    decimal SaldoAtual,
    decimal? ValorUnitario);

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

    Task<decimal> GetSaldoByLoteAsync(
        Guid empresaId,
        Guid loteProdutoId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LotBalanceRow>> ListLotBalancesAsync(
        Guid empresaId,
        Guid? unidadeId,
        Guid? produtoId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<(Guid LoteProdutoId, DateOnly DataValidade, DateTime CriadoEm, decimal Saldo)>> ListLotsWithBalanceFefoAsync(
        Guid empresaId,
        Guid unidadeId,
        Guid produtoId,
        CancellationToken cancellationToken = default);
}
