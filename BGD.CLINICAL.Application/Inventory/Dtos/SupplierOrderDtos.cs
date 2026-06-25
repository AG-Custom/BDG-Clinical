namespace BGD.CLINICAL.Application.Inventory.Dtos;

public sealed record SupplierOrderItemDto(
    Guid Id,
    Guid ProdutoId,
    string ProdutoNome,
    decimal Quantidade,
    decimal ValorUnitario,
    decimal ValorTotal);

public sealed record SupplierOrderDto(
    Guid Id,
    Guid FornecedorId,
    string FornecedorNome,
    Guid UnidadeId,
    string UnidadeNome,
    string TipoPedido,
    DateTime DataPedido,
    string Status,
    decimal ValorTotal,
    string? Observacao,
    IReadOnlyList<SupplierOrderItemDto> Itens,
    DateTime CriadoEm,
    DateTime? AtualizadoEm);

public sealed record SupplierOrderItemRequest(
    Guid ProdutoId,
    decimal Quantidade,
    decimal? ValorUnitario = null,
    decimal? ValorTotal = null);

public sealed record CreateSupplierOrderRequest(
    Guid FornecedorId,
    Guid UnidadeId,
    string TipoPedido,
    DateTime DataPedido,
    string Status,
    string? Observacao,
    IReadOnlyList<SupplierOrderItemRequest> Itens);

public sealed record UpdateSupplierOrderRequest(
    Guid FornecedorId,
    Guid UnidadeId,
    string TipoPedido,
    DateTime DataPedido,
    string Status,
    string? Observacao,
    IReadOnlyList<SupplierOrderItemRequest> Itens);
