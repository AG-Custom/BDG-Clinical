using BGD.CLINICAL.Domain.Enums;

namespace BGD.CLINICAL.Application.Inventory.MeasurementUnits;

public static class DefaultMeasurementUnitsCatalog
{
    public static IReadOnlyList<DefaultMeasurementUnitDefinition> All { get; } =
    [
        new("Miligrama", "mg", TipoUnidadeMedida.Massa),
        new("Grama", "g", TipoUnidadeMedida.Massa),
        new("Quilograma", "kg", TipoUnidadeMedida.Massa),
        new("Mililitro", "ml", TipoUnidadeMedida.Volume),
        new("Litro", "L", TipoUnidadeMedida.Volume),
        new("Unidade", "un", TipoUnidadeMedida.Unidade),
        new("Caixa", "cx", TipoUnidadeMedida.Embalagem),
        new("Frasco", "fr", TipoUnidadeMedida.Embalagem),
        new("Ampola", "amp", TipoUnidadeMedida.Embalagem),
    ];
}
