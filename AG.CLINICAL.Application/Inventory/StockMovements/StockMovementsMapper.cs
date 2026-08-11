using AG.CLINICAL.Application.Inventory;
using AG.CLINICAL.Application.Inventory.Dtos;
using AG.CLINICAL.Domain.Entities;

namespace AG.CLINICAL.Application.Inventory.StockMovements;

internal static class StockMovementsMapper
{
    public static StockMovementDto Map(
        MovimentacaoEstoque movimentacao,
        IReadOnlyDictionary<(Guid PedidoId, Guid ProdutoId), decimal>? valoresPedido = null)
    {
        var valorUnitario = ResolverValorUnitario(movimentacao, valoresPedido);
        var valorTotal = Math.Round(
            movimentacao.Quantidade * valorUnitario,
            2,
            MidpointRounding.AwayFromZero);

        return new StockMovementDto(
            movimentacao.Id,
            movimentacao.UnidadeId,
            movimentacao.Unidade?.Nome ?? string.Empty,
            movimentacao.ProdutoId,
            movimentacao.Produto?.Nome ?? string.Empty,
            movimentacao.LoteProdutoId,
            movimentacao.LoteProduto?.Codigo,
            movimentacao.LoteProduto?.DataValidade,
            movimentacao.Tipo.ToString(),
            movimentacao.Motivo.ToString(),
            movimentacao.Quantidade,
            valorUnitario,
            valorTotal,
            movimentacao.QuantidadeEmbalagem,
            movimentacao.Data,
            movimentacao.Origem,
            movimentacao.PedidoFornecedorId,
            movimentacao.AplicacaoPacienteId,
            movimentacao.TransferenciaEstoqueId,
            movimentacao.Observacao,
            movimentacao.CriadoEm);
    }

    public static IReadOnlyList<StockMovementDto> Map(
        IReadOnlyList<MovimentacaoEstoque> movimentacoes,
        IReadOnlyDictionary<(Guid PedidoId, Guid ProdutoId), decimal>? valoresPedido = null)
    {
        return movimentacoes
            .Select(movimentacao => Map(movimentacao, valoresPedido))
            .ToList();
    }

    private static decimal ResolverValorUnitario(
        MovimentacaoEstoque movimentacao,
        IReadOnlyDictionary<(Guid PedidoId, Guid ProdutoId), decimal>? valoresPedido)
    {
        if (movimentacao.ValorUnitario.HasValue)
        {
            return movimentacao.ValorUnitario.Value;
        }

        decimal valorEmbalagemOuCadastro;

        if (
            movimentacao.PedidoFornecedorId is Guid pedidoId
            && valoresPedido is not null
            && valoresPedido.TryGetValue((pedidoId, movimentacao.ProdutoId), out var valorPedido))
        {
            valorEmbalagemOuCadastro = valorPedido;
        }
        else
        {
            valorEmbalagemOuCadastro = movimentacao.Produto?.Valor ?? 0m;
        }

        return ProductStockValuation.ResolveValorPorUnidadeEstoque(
            valorEmbalagemOuCadastro,
            movimentacao.Produto?.FatorEmbalagemParaEstoque);
    }
}
