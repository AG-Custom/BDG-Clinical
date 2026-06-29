using System.Text.Json;
using System.Text.Json.Serialization;

namespace BGD.CLINICAL.WebApi.Controllers.SupplierOrder;

internal static class SupplierOrderControllerJson
{
    internal static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };
}
