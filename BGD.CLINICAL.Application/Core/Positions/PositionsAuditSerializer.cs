using System.Text.Json;
using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Application.Core.Positions;

internal static class PositionsAuditSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static string Serialize(Cargo cargo)
    {
        return JsonSerializer.Serialize(new
        {
            cargo.Id,
            cargo.EmpresaId,
            cargo.Nome,
            cargo.Ativo,
        }, Options);
    }
}
