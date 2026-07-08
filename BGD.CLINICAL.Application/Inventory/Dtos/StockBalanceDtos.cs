namespace BGD.CLINICAL.Application.Inventory.Dtos;

public sealed record StockBalanceDto(
    Guid UnidadeId,
    string UnidadeNome,
    Guid ProdutoId,
    string ProdutoNome,
    string UnidadeMedidaSigla,
    decimal EstoqueMinimo,
    decimal SaldoAtual,
    decimal? ValorUnitario,
    decimal ValorEstoque,
    bool AbaixoDoMinimo);
