using System.Text.Json;
using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Application.Inventory.ProductTypes;

internal static class ProductTypesAuditSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static string Serialize(TipoProduto tipoProduto)
    {
        return JsonSerializer.Serialize(new
        {
            tipoProduto.Id,
            tipoProduto.EmpresaId,
            tipoProduto.Nome,
            tipoProduto.Codigo,
            tipoProduto.Ativo,
        }, Options);
    }
}
