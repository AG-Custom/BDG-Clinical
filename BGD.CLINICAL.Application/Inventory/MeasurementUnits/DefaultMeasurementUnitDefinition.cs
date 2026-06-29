using BGD.CLINICAL.Domain.Enums;

namespace BGD.CLINICAL.Application.Inventory.MeasurementUnits;

public sealed record DefaultMeasurementUnitDefinition(
    string Nome,
    string Sigla,
    TipoUnidadeMedida Tipo);
