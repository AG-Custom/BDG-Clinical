<<<<<<< HEAD:AG.CLINICAL.Application/Applications/PatientApplications/PatientApplicationStockPlanner.cs
using AG.CLINICAL.Domain.Entities;
=======
using BGD.CLINICAL.Application.Applications.Dtos;
using BGD.CLINICAL.Domain.Entities;
>>>>>>> beaf46413336e75715ca7c780d9d8eb24c23c7ae:BGD.CLINICAL.Application/Applications/PatientApplications/PatientApplicationStockPlanner.cs

namespace AG.CLINICAL.Application.Applications.PatientApplications;

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
        IReadOnlyDictionary<Guid, Produto> productsById,
        bool consumirInsumosKit = true,
        IReadOnlyList<PatientApplicationManualSupplyRequest>? insumosManuais = null)
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

        if (consumirInsumosKit)
        {
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

        if (insumosManuais is { Count: > 0 })
        {
            foreach (var item in insumosManuais)
            {
                var produto = productsById[item.ProdutoId];
                lines.Add(new StockConsumptionLine(
                    produto.Id,
                    produto.Nome,
                    item.Quantidade,
                    produto.ControlaEstoque));
            }
        }

        return lines;
    }
}
