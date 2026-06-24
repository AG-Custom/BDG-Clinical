using System.Text.Json;
using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Application.Core.Units;

internal static class UnitsAuditSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static string Serialize(Unidade unidade)
    {
        return JsonSerializer.Serialize(new
        {
            unidade.Id,
            unidade.EmpresaId,
            unidade.Nome,
            unidade.Endereco,
            unidade.Ativo,
        }, Options);
    }
}
