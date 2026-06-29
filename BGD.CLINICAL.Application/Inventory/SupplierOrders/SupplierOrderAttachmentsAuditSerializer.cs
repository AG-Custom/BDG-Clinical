using System.Text.Json;
using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Application.Inventory.SupplierOrders;

internal static class SupplierOrderAttachmentsAuditSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static string Serialize(AnexoPedidoFornecedor anexo)
    {
        return JsonSerializer.Serialize(new
        {
            anexo.Id,
            anexo.PedidoFornecedorId,
            anexo.EmpresaId,
            anexo.NomeArquivo,
            anexo.ContentType,
            anexo.ObjectKey,
            anexo.TamanhoBytes,
        }, Options);
    }
}
