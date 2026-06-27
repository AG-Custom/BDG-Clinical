using System.Text.RegularExpressions;
using BGD.CLINICAL.Domain.Common;

namespace BGD.CLINICAL.Domain.ValueObjects;

public sealed partial class Email : ValueObject
{
    private static readonly Regex EmailPattern = EmailFormat();

    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static string? Normalize(string? raw)
    {
        return string.IsNullOrWhiteSpace(raw) ? null : raw.Trim();
    }

    public static string? Validate(string? raw)
    {
        var normalized = Normalize(raw);

        if (normalized is null)
        {
            return null;
        }

        if (!EmailPattern.IsMatch(normalized))
        {
            return "Informe um e-mail válido.";
        }

        return null;
    }

    public static Email? TryCreate(string? raw, out string? error)
    {
        error = Validate(raw);

        if (error is not null)
        {
            return null;
        }

        var normalized = Normalize(raw);
        return normalized is null ? null : new Email(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase)]
    private static partial Regex EmailFormat();
}
