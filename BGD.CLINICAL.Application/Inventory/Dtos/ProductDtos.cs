namespace BGD.CLINICAL.Application.Inventory.Dtos;

public sealed record ProductDto(
    Guid Id,
    Guid TipoProdutoId,
    string TipoProdutoNome,
    Guid UnidadeMedidaId,
    string UnidadeMedidaNome,
    string UnidadeMedidaSigla,
    string Nome,
    string? Sku,
    string? CodigoInterno,
    string? CodigoBarras,
    decimal EstoqueMinimo,
    decimal Valor,
    bool ControlaEstoque,
    bool Ativo,
    DateTime CriadoEm,
    DateTime? AtualizadoEm);

public sealed record CreateProductRequest(
    Guid TipoProdutoId,
    Guid UnidadeMedidaId,
    string Nome,
    decimal EstoqueMinimo,
    decimal Valor,
    string? Sku = null,
    string? CodigoInterno = null,
    string? CodigoBarras = null,
    bool ControlaEstoque = true);

public sealed record UpdateProductRequest(
    Guid TipoProdutoId,
    Guid UnidadeMedidaId,
    string Nome,
    decimal EstoqueMinimo,
    decimal Valor,
    string? Sku = null,
    string? CodigoInterno = null,
    string? CodigoBarras = null,
    bool ControlaEstoque = true);
