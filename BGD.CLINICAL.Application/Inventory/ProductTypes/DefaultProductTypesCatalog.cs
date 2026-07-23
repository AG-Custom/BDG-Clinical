using BGD.CLINICAL.Domain.Constants;

namespace BGD.CLINICAL.Application.Inventory.ProductTypes;

public static class DefaultProductTypesCatalog
{
    public static IReadOnlyList<DefaultProductTypeDefinition> All { get; } =
    [
        new("Medicamento", ProductTypeCodes.Medicamento),
        new("Insumo", ProductTypeCodes.Insumo),
    ];
}
