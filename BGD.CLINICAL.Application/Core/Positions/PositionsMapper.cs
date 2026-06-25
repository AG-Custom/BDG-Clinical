using BGD.CLINICAL.Application.Core.Dtos;
using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Application.Core.Positions;

internal static class PositionsMapper
{
    public static PositionDto Map(Cargo cargo)
    {
        return new PositionDto(
            cargo.Id,
            cargo.Nome,
            cargo.Ativo,
            cargo.CriadoEm,
            cargo.AtualizadoEm);
    }

    public static IReadOnlyList<PositionDto> Map(IReadOnlyList<Cargo> cargos)
    {
        return cargos.Select(Map).ToList();
    }
}
