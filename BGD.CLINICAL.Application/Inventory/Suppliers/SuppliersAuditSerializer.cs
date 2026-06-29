using System.Text.Json;
using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Application.Inventory.Suppliers;

internal static class SuppliersAuditSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static string Serialize(Fornecedor fornecedor)
    {
        return JsonSerializer.Serialize(new
        {
            fornecedor.Id,
            fornecedor.EmpresaId,
            fornecedor.Nome,
            fornecedor.Cnpj,
            fornecedor.Telefone,
            fornecedor.Email,
            fornecedor.Observacao,
            fornecedor.Ativo,
        }, Options);
    }
}
