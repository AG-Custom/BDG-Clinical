using System.Text.Json;
using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Application.Inventory.Products;

internal static class ProductsAuditSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static string Serialize(Produto produto)
    {
        return JsonSerializer.Serialize(new
        {
            produto.Id,
            produto.EmpresaId,
            produto.TipoProdutoId,
            produto.UnidadeMedidaId,
            produto.Nome,
            produto.EstoqueMinimo,
            produto.Ativo,
        }, Options);
    }
}
