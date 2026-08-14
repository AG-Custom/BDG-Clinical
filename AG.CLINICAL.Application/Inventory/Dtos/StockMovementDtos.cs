namespace AG.CLINICAL.Application.Inventory.Dtos;

public sealed record StockMovementDto(
    Guid Id,
    Guid UnidadeId,
    string UnidadeNome,
    Guid ProdutoId,
    string ProdutoNome,
    Guid? LoteProdutoId,
    string? LoteCodigo,
    DateOnly? LoteDataValidade,
    string Tipo,
    string Motivo,
    decimal Quantidade,
    decimal ValorUnitario,
    decimal ValorTotal,
    decimal? QuantidadeEmbalagem,
    DateTime Data,
    string Origem,
    Guid? PedidoFornecedorId,
    Guid? AplicacaoPacienteId,
    Guid? TransferenciaEstoqueId,
    string? Observacao,
    DateTime CriadoEm);

public sealed record CreateManualStockMovementRequest(
    Guid UnidadeId,
    Guid ProdutoId,
    DateTime Data,
    decimal? Quantidade = null,
    string? Observacao = null,
    decimal? QuantidadeEmbalagem = null,
    string? LoteCodigo = null,
    DateOnly? DataValidade = null,
    decimal? ValorUnitario = null,
    IReadOnlyList<CreateManualStockMovementLotRequest>? Lotes = null);

public sealed record CreateManualStockMovementLotRequest(
    string LoteCodigo,
    decimal QuantidadeEmbalagem,
    DateOnly DataValidade);

public sealed record CreateStockTransferRequest(
    Guid UnidadeOrigemId,
    Guid UnidadeDestinoId,
    Guid ProdutoId,
    decimal Quantidade,
    DateTime Data,
    string? Observacao = null);

public sealed record StockTransferDto(
    Guid TransferenciaId,
    Guid UnidadeOrigemId,
    string UnidadeOrigemNome,
    Guid UnidadeDestinoId,
    string UnidadeDestinoNome,
    Guid ProdutoId,
    string ProdutoNome,
    decimal Quantidade,
    DateTime Data,
    string? Observacao,
    IReadOnlyList<StockMovementDto> Movimentacoes);

public sealed record ReceiveSupplierOrderRequest(
    IReadOnlyList<ReceiveSupplierOrderItemLotRequest>? Itens = null);

public sealed record ReceiveSupplierOrderItemLotRequest(
    Guid ProdutoId,
    string? LoteCodigo = null,
    DateOnly? DataValidade = null,
    decimal? QuantidadeEmbalagem = null);

public sealed record StockLotBalanceDto(
    Guid LoteProdutoId,
    Guid UnidadeId,
    string UnidadeNome,
    Guid ProdutoId,
    string ProdutoNome,
    string Codigo,
    DateOnly DataValidade,
    decimal SaldoAtual,
    string UnidadeMedidaSigla,
    decimal? FatorEmbalagemParaEstoque,
    decimal? SaldoEmbalagem);
