using System.Text.RegularExpressions;

namespace BGD.CLINICAL.Application.Patients.Patients;

internal static class PatientValidation
{
    private static readonly Regex CpfDigitsOnlyPattern = new(@"^\d{11}$", RegexOptions.Compiled);

    public static string? NormalizeCpf(string? cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
        {
            return null;
        }

        var digits = new string(cpf.Where(char.IsDigit).ToArray());
        return string.IsNullOrEmpty(digits) ? null : digits;
    }

    public static string? ValidateCpf(string? cpf)
    {
        var normalized = NormalizeCpf(cpf);

        if (normalized is null)
        {
            return null;
        }

        if (!CpfDigitsOnlyPattern.IsMatch(normalized))
        {
            return "Informe um CPF válido com 11 dígitos.";
        }

        return null;
    }

    public static string? NormalizeTelefone(string? telefone)
    {
        return string.IsNullOrWhiteSpace(telefone) ? null : telefone.Trim();
    }

    public static string? NormalizeEmail(string? email)
    {
        return string.IsNullOrWhiteSpace(email) ? null : email.Trim();
    }

    public static string? NormalizeObservacao(string? observacao)
    {
        return string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim();
    }
}
