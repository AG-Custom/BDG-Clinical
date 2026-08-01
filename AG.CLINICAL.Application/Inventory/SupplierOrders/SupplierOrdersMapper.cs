using AG.CLINICAL.Application.Abstractions.Storage;
using AG.CLINICAL.Application.Inventory.Dtos;
using AG.CLINICAL.Domain.Entities;
using AG.CLINICAL.Domain.Enums;

namespace AG.CLINICAL.Application.Inventory.SupplierOrders;

internal static class SupplierOrdersMapper
{
    public static SupplierOrderDto Map(
        PedidoFornecedor pedido,
        IObjectStorageService? objectStorage = null)
    {
        return new SupplierOrderDto(
            pedido.Id,
            pedido.FornecedorId,
            pedido.Fornecedor?.Nome ?? string.Empty,
            pedido.UnidadeId,
            pedido.Unidade?.Nome ?? string.Empty,
            pedido.TipoPedido.ToString(),
            pedido.DataPedido,
            pedido.Status.ToApiString(),
            pedido.ValorTotal,
            pedido.Observacao,
            MapItens(pedido.Itens),
            MapAnexos(pedido.Anexos, objectStorage),
            pedido.CriadoEm,
            pedido.AtualizadoEm);
    }

    public static IReadOnlyList<SupplierOrderDto> Map(
        IReadOnlyList<PedidoFornecedor> pedidos,
        IObjectStorageService? objectStorage = null)
    {
        return pedidos.Select(pedido => Map(pedido, objectStorage)).ToList();
    }

    private static IReadOnlyList<SupplierOrderItemDto> MapItens(
        IReadOnlyCollection<ItemPedidoFornecedor> itens)
    {
        return itens
            .Select(item => new SupplierOrderItemDto(
                item.Id,
                item.ProdutoId,
                item.Produto?.Nome ?? string.Empty,
                item.Quantidade,
                item.ValorUnitario,
                item.ValorTotal))
            .ToList();
    }

    private static IReadOnlyList<SupplierOrderAttachmentDto> MapAnexos(
        IEnumerable<AnexoPedidoFornecedor> anexos,
        IObjectStorageService? objectStorage)
    {
        if (objectStorage is null)
        {
            return [];
        }

        return SupplierOrderAttachmentsMapper.Map(anexos, objectStorage);
    }
}
