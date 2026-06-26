using System.Text.Json;
using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Application.Inventory.StockMovements;

internal static class StockMovementsAuditSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static string Serialize(MovimentacaoEstoque movimentacao)
    {
        return JsonSerializer.Serialize(new
        {
            movimentacao.Id,
            movimentacao.EmpresaId,
            movimentacao.UnidadeId,
            movimentacao.ProdutoId,
            Tipo = movimentacao.Tipo.ToString(),
            movimentacao.Quantidade,
            movimentacao.Data,
            movimentacao.Origem,
            movimentacao.FuncionarioId,
            movimentacao.Observacao,
        }, Options);
    }
}
