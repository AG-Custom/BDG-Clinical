using BGD.CLINICAL.Application.Inventory.Dtos;
using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Application.Inventory.Products;

internal static class ProductsMapper
{
    public static ProductDto Map(Produto produto)
    {
        return new ProductDto(
            produto.Id,
            produto.TipoProdutoId,
            produto.TipoProduto?.Nome ?? string.Empty,
            produto.UnidadeMedidaId,
            produto.UnidadeMedida?.Nome ?? string.Empty,
            produto.UnidadeMedida?.Sigla ?? string.Empty,
            produto.Nome,
            produto.Sku,
            produto.CodigoInterno,
            produto.CodigoBarras,
            produto.EstoqueMinimo,
            produto.ControlaEstoque,
            produto.Ativo,
            produto.CriadoEm,
            produto.AtualizadoEm);
    }

    public static IReadOnlyList<ProductDto> Map(IReadOnlyList<Produto> produtos)
    {
        return produtos.Select(Map).ToList();
    }
}
