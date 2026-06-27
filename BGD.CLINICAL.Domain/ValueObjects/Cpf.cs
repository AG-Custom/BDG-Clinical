using System.Text.RegularExpressions;
using BGD.CLINICAL.Domain.Common;

namespace BGD.CLINICAL.Domain.ValueObjects;

public sealed partial class Cpf : ValueObject
{
    private static readonly Regex DigitsOnlyPattern = DigitsOnly();

    public string Value { get; }

    private Cpf(string value)
    {
        Value = value;
    }

    public static string? Normalize(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var digits = new string(raw.Where(char.IsDigit).ToArray());
        return string.IsNullOrEmpty(digits) ? null : digits;
    }

    public static string? Validate(string? raw)
    {
        var normalized = Normalize(raw);

        if (normalized is null)
        {
            return null;
        }

        if (!DigitsOnlyPattern.IsMatch(normalized))
        {
            return "Informe um CPF válido com 11 dígitos.";
        }

        return null;
    }

    public static Cpf? TryCreate(string? raw, out string? error)
    {
        error = Validate(raw);

        if (error is not null)
        {
            return null;
        }

        var normalized = Normalize(raw);
        return normalized is null ? null : new Cpf(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^\d{11}$")]
    private static partial Regex DigitsOnly();
}
