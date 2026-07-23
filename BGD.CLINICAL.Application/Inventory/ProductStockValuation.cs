namespace BGD.CLINICAL.Application.Inventory;

public static class ProductStockValuation
{
    public static decimal? ResolveFatorEmbalagemParaEstoque(
        decimal? conteudoPorEmbalagem,
        decimal? concentracaoPorConteudo)
    {
        if (conteudoPorEmbalagem is > 0 && concentracaoPorConteudo is > 0)
        {
            return conteudoPorEmbalagem.Value * concentracaoPorConteudo.Value;
        }

        return null;
    }

    public static decimal ResolveValorPorUnidadeEstoque(
        decimal valorCadastroOuPedido,
        decimal? fatorEmbalagemParaEstoque)
    {
        if (fatorEmbalagemParaEstoque is > 0)
        {
            return valorCadastroOuPedido / fatorEmbalagemParaEstoque.Value;
        }

        return valorCadastroOuPedido;
    }
}
