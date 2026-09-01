using System.Text.Json;
using AG.CLINICAL.Domain.Entities;

namespace AG.CLINICAL.Application.Schedules.AppointmentTags;

internal static class AppointmentTagsAuditSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static string Serialize(TagAgendamento tag)
    {
        return JsonSerializer.Serialize(new
        {
            tag.Id,
            tag.EmpresaId,
            tag.Nome,
            tag.Cor,
            tag.Ativo,
        }, Options);
    }
}
