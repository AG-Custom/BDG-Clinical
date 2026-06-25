using System.Text.Json;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;

namespace BGD.CLINICAL.Application.Inventory.SupplierOrders;

internal static class SupplierOrdersAuditSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static string Serialize(PedidoFornecedor pedido)
    {
        return JsonSerializer.Serialize(new
        {
            pedido.Id,
            pedido.EmpresaId,
            pedido.FornecedorId,
            pedido.UnidadeId,
            TipoPedido = pedido.TipoPedido.ToString(),
            pedido.DataPedido,
            Status = pedido.Status.ToApiString(),
            pedido.ValorTotal,
            pedido.Observacao,
            Itens = pedido.Itens.Select(item => new
            {
                item.Id,
                item.ProdutoId,
                item.Quantidade,
                item.ValorUnitario,
                item.ValorTotal,
            }),
        }, Options);
    }

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
            movimentacao.PedidoFornecedorId,
        }, Options);
    }
}
