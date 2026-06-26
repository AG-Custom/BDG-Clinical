using BGD.CLINICAL.Application.Patients.Dtos;
using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Application.Patients.Symptoms;

internal static class SymptomsMapper
{
    public static SymptomDto Map(Sintoma sintoma)
    {
        return new SymptomDto(
            sintoma.Id,
            sintoma.Nome,
            sintoma.Ativo,
            sintoma.CriadoEm,
            sintoma.AtualizadoEm);
    }

    public static IReadOnlyList<SymptomDto> Map(IReadOnlyList<Sintoma> sintomas)
    {
        return sintomas.Select(Map).ToList();
    }
}
