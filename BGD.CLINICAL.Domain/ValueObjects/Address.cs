using System.Text.RegularExpressions;
using BGD.CLINICAL.Domain.Common;

namespace BGD.CLINICAL.Domain.ValueObjects;

public sealed partial class Address : ValueObject
{
    private static readonly Regex CepPattern = CepDigits();
    private static readonly Regex UfPattern = UfLetters();

    private Address()
    {
    }

    private Address(
        string? cep,
        string? logradouro,
        string? numero,
        string? complemento,
        string? bairro,
        string? cidade,
        string? uf)
    {
        Cep = cep;
        Logradouro = logradouro;
        Numero = numero;
        Complemento = complemento;
        Bairro = bairro;
        Cidade = cidade;
        Uf = uf;
    }

    public string? Cep { get; private set; }
    public string? Logradouro { get; private set; }
    public string? Numero { get; private set; }
    public string? Complemento { get; private set; }
    public string? Bairro { get; private set; }
    public string? Cidade { get; private set; }
    public string? Uf { get; private set; }

    public static string? NormalizeText(string? raw)
    {
        return string.IsNullOrWhiteSpace(raw) ? null : raw.Trim();
    }

    public static string? NormalizeCep(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var digits = new string(raw.Where(char.IsDigit).ToArray());
        return string.IsNullOrEmpty(digits) ? null : digits;
    }

    public static string? NormalizeUf(string? raw)
    {
        return string.IsNullOrWhiteSpace(raw) ? null : raw.Trim().ToUpperInvariant();
    }

    public static string? Validate(
        string? cep,
        string? logradouro,
        string? numero,
        string? complemento,
        string? bairro,
        string? cidade,
        string? uf)
    {
        var normalizedCep = NormalizeCep(cep);
        if (normalizedCep is not null && !CepPattern.IsMatch(normalizedCep))
        {
            return "Informe um CEP válido com 8 dígitos.";
        }

        var normalizedUf = NormalizeUf(uf);
        if (normalizedUf is not null && !UfPattern.IsMatch(normalizedUf))
        {
            return "Informe uma UF válida com 2 letras.";
        }

        return null;
    }

    public static Address? TryCreate(
        string? cep,
        string? logradouro,
        string? numero,
        string? complemento,
        string? bairro,
        string? cidade,
        string? uf,
        out string? error)
    {
        error = Validate(cep, logradouro, numero, complemento, bairro, cidade, uf);
        if (error is not null)
        {
            return null;
        }

        var normalizedCep = NormalizeCep(cep);
        var normalizedLogradouro = NormalizeText(logradouro);
        var normalizedNumero = NormalizeText(numero);
        var normalizedComplemento = NormalizeText(complemento);
        var normalizedBairro = NormalizeText(bairro);
        var normalizedCidade = NormalizeText(cidade);
        var normalizedUf = NormalizeUf(uf);

        if (normalizedCep is null
            && normalizedLogradouro is null
            && normalizedNumero is null
            && normalizedComplemento is null
            && normalizedBairro is null
            && normalizedCidade is null
            && normalizedUf is null)
        {
            return null;
        }

        return new Address(
            normalizedCep,
            normalizedLogradouro,
            normalizedNumero,
            normalizedComplemento,
            normalizedBairro,
            normalizedCidade,
            normalizedUf);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Cep;
        yield return Logradouro;
        yield return Numero;
        yield return Complemento;
        yield return Bairro;
        yield return Cidade;
        yield return Uf;
    }

    [GeneratedRegex(@"^\d{8}$")]
    private static partial Regex CepDigits();

    [GeneratedRegex(@"^[A-Z]{2}$")]
    private static partial Regex UfLetters();
}
