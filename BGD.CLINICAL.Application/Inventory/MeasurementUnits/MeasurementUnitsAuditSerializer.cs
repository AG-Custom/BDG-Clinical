using System.Text.Json;
using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Application.Inventory.MeasurementUnits;

internal static class MeasurementUnitsAuditSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static string Serialize(UnidadeMedida unidadeMedida)
    {
        return JsonSerializer.Serialize(new
        {
            unidadeMedida.Id,
            unidadeMedida.EmpresaId,
            unidadeMedida.Nome,
            unidadeMedida.Sigla,
            Tipo = unidadeMedida.Tipo.ToString(),
            unidadeMedida.Ativo,
        }, Options);
    }
}
