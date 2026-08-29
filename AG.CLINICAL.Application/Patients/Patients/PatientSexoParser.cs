using AG.CLINICAL.Domain.Enums;

namespace AG.CLINICAL.Application.Patients.Patients;

internal static class PatientSexoParser
{
    public static Sexo? Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Enum.TryParse<Sexo>(value.Trim(), ignoreCase: true, out var sexo)
            ? sexo
            : null;
    }
}
