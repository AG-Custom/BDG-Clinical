namespace BGD.CLINICAL.Application.Inventory.Dtos;

public sealed record MeasurementUnitDto(
    Guid Id,
    string Nome,
    string Sigla,
    string Tipo,
    bool Ativo,
    DateTime CriadoEm,
    DateTime? AtualizadoEm);

public sealed record CreateMeasurementUnitRequest(
    string Nome,
    string Sigla,
    string Tipo);

public sealed record UpdateMeasurementUnitRequest(
    string Nome,
    string Sigla,
    string Tipo);
