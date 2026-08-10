using AG.CLINICAL.Application.Core.Dtos;
using AG.CLINICAL.Domain.Entities;

namespace AG.CLINICAL.Application.Core.Units;

internal static class UnitsMapper
{
    public static UnitDto Map(Unidade unidade)
    {
        return new UnitDto(
            unidade.Id,
            unidade.Nome,
            unidade.Endereco,
            unidade.Ativo,
            unidade.CriadoEm,
            unidade.AtualizadoEm);
    }

    public static IReadOnlyList<UnitDto> Map(IReadOnlyList<Unidade> unidades)
    {
        return unidades.Select(Map).ToList();
    }
}
