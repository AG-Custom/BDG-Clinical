using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Application.Applications.PatientApplications;

internal sealed record StockConsumptionLine(
    Guid ProdutoId,
    string ProdutoNome,
    decimal Quantidade,
    bool ControlaEstoque);

internal static class PatientApplicationStockPlanner
{
    public static IReadOnlyList<StockConsumptionLine> BuildLines(
        decimal? quantidadeUtilizada,
        Procedimento procedimento,
        IReadOnlyDictionary<Guid, Produto> productsById)
    {
        var lines = new List<StockConsumptionLine>();

        if (procedimento.ProdutoAplicadoId.HasValue && quantidadeUtilizada.HasValue)
        {
            var produto = productsById[procedimento.ProdutoAplicadoId.Value];
            lines.Add(new StockConsumptionLine(
                produto.Id,
                produto.Nome,
                quantidadeUtilizada.Value,
                produto.ControlaEstoque));
        }

        foreach (var item in procedimento.Itens)
        {
            var produto = productsById[item.ProdutoId];
            lines.Add(new StockConsumptionLine(
                produto.Id,
                produto.Nome,
                item.Quantidade,
                produto.ControlaEstoque));
        }

        return lines;
    }
}
