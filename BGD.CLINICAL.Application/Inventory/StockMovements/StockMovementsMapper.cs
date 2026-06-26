using BGD.CLINICAL.Application.Inventory.Dtos;
using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Application.Inventory.StockMovements;

internal static class StockMovementsMapper
{
    public static StockMovementDto Map(MovimentacaoEstoque movimentacao)
    {
        return new StockMovementDto(
            movimentacao.Id,
            movimentacao.UnidadeId,
            movimentacao.Unidade?.Nome ?? string.Empty,
            movimentacao.ProdutoId,
            movimentacao.Produto?.Nome ?? string.Empty,
            movimentacao.Tipo.ToString(),
            movimentacao.Motivo.ToString(),
            movimentacao.Quantidade,
            movimentacao.Data,
            movimentacao.Origem,
            movimentacao.PedidoFornecedorId,
            movimentacao.AplicacaoPacienteId,
            movimentacao.Observacao,
            movimentacao.CriadoEm);
    }

    public static IReadOnlyList<StockMovementDto> Map(IReadOnlyList<MovimentacaoEstoque> movimentacoes)
    {
        return movimentacoes
            .Select(Map)
            .ToList();
    }
}
