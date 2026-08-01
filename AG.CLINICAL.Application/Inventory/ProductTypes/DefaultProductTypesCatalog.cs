using AG.CLINICAL.Domain.Constants;

namespace AG.CLINICAL.Application.Inventory.ProductTypes;

public static class DefaultProductTypesCatalog
{
    public static IReadOnlyList<DefaultProductTypeDefinition> All { get; } =
    [
        new("Medicamento", ProductTypeCodes.Medicamento),
        new("Insumo", ProductTypeCodes.Insumo),
    ];
}
