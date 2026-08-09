using AG.CLINICAL.Application.Inventory.Dtos;
using AG.CLINICAL.Domain.Entities;

namespace AG.CLINICAL.Application.Inventory.MeasurementUnits;

internal static class MeasurementUnitsMapper
{
    public static MeasurementUnitDto Map(UnidadeMedida unidadeMedida)
    {
        return new MeasurementUnitDto(
            unidadeMedida.Id,
            unidadeMedida.Nome,
            unidadeMedida.Sigla,
            unidadeMedida.Tipo.ToString(),
            unidadeMedida.Ativo,
            unidadeMedida.CriadoEm,
            unidadeMedida.AtualizadoEm);
    }

    public static IReadOnlyList<MeasurementUnitDto> Map(IReadOnlyList<UnidadeMedida> unidadesMedida)
    {
        return unidadesMedida.Select(Map).ToList();
    }
}
