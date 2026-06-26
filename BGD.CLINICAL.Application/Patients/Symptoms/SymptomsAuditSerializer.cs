using System.Text.Json;
using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Application.Patients.Symptoms;

internal static class SymptomsAuditSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static string Serialize(Sintoma sintoma)
    {
        return JsonSerializer.Serialize(new
        {
            sintoma.Id,
            sintoma.EmpresaId,
            sintoma.Nome,
            sintoma.Ativo,
        }, Options);
    }
}
