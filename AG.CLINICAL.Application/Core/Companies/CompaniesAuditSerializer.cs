using System.Text.Json;
using AG.CLINICAL.Domain.Entities;

namespace AG.CLINICAL.Application.Core.Companies;

internal static class CompaniesAuditSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static string Serialize(Empresa empresa)
    {
        return JsonSerializer.Serialize(new
        {
            empresa.Id,
            empresa.Nome,
            empresa.Cnpj,
            empresa.Telefone,
            empresa.Email,
            empresa.Logo,
            empresa.CorPrincipal,
            empresa.Ativo,
        }, Options);
    }
}
