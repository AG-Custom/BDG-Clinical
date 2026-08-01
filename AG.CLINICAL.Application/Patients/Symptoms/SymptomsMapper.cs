using AG.CLINICAL.Application.Patients.Dtos;
using AG.CLINICAL.Domain.Entities;

namespace AG.CLINICAL.Application.Patients.Symptoms;

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
