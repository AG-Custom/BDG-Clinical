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
            produto.TipoProduto?.Codigo,
            produto.UnidadeMedidaId,
            produto.UnidadeMedida?.Nome ?? string.Empty,
            produto.UnidadeMedida?.Sigla ?? string.Empty,
            produto.UnidadeEmbalagemId,
            produto.UnidadeEmbalagem?.Nome,
            produto.UnidadeEmbalagem?.Sigla,
            produto.ConteudoPorEmbalagem,
            produto.UnidadeConteudoId,
            produto.UnidadeConteudo?.Nome,
            produto.UnidadeConteudo?.Sigla,
            produto.ConcentracaoPorConteudo,
            produto.FatorEmbalagemParaEstoque,
            produto.Nome,
            produto.Sku,
            produto.CodigoInterno,
            produto.CodigoBarras,
            produto.EstoqueMinimo,
            produto.Valor,
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
