using AG.CLINICAL.Domain.Enums;

namespace AG.CLINICAL.Application.Inventory.MeasurementUnits;

public sealed record DefaultMeasurementUnitDefinition(
    string Nome,
    string Sigla,
    TipoUnidadeMedida Tipo);
