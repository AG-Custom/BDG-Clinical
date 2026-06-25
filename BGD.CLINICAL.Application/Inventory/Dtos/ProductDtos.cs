namespace BGD.CLINICAL.Application.Inventory.Dtos;

public sealed record ProductDto(
    Guid Id,
    Guid TipoProdutoId,
    string TipoProdutoNome,
    Guid UnidadeMedidaId,
    string UnidadeMedidaNome,
    string UnidadeMedidaSigla,
    string Nome,
    decimal EstoqueMinimo,
    bool Ativo,
    DateTime CriadoEm,
    DateTime? AtualizadoEm);

public sealed record CreateProductRequest(
    Guid TipoProdutoId,
    Guid UnidadeMedidaId,
    string Nome,
    decimal EstoqueMinimo);

public sealed record UpdateProductRequest(
    Guid TipoProdutoId,
    Guid UnidadeMedidaId,
    string Nome,
    decimal EstoqueMinimo);
