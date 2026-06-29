using System.Text.RegularExpressions;
using BGD.CLINICAL.Application.Common;

namespace BGD.CLINICAL.Application.Inventory.Suppliers;

internal static class SupplierSearchOptions
{
    public const int DefaultLimit = 20;
    public const int MaxLimit = 50;
    public const int MinSearchLength = 2;
}

internal static class SupplierValidation
{
    private static readonly Regex CnpjDigitsOnlyPattern = new(@"^\d{14}$", RegexOptions.Compiled);

    public static string? NormalizeCnpj(string? cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
        {
            return null;
        }

        var digits = new string(cnpj.Where(char.IsDigit).ToArray());
        return string.IsNullOrEmpty(digits) ? null : digits;
    }

    public static string? ValidateCnpj(string? cnpj)
    {
        var normalized = NormalizeCnpj(cnpj);

        if (normalized is null)
        {
            return "Informe o CNPJ do fornecedor.";
        }

        if (!CnpjDigitsOnlyPattern.IsMatch(normalized))
        {
            return "Informe um CNPJ válido com 14 dígitos.";
        }

        return null;
    }

    public static string? NormalizeNome(string? nome)
    {
        return string.IsNullOrWhiteSpace(nome) ? null : nome.Trim();
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

    public static string? ValidateObservacao(string? observacao)
    {
        if (string.IsNullOrWhiteSpace(observacao))
        {
            return null;
        }

        if (observacao.Trim().Length > 2000)
        {
            return "A observação deve ter no máximo 2000 caracteres.";
        }

        return null;
    }

    public static Result<int> ValidateLimit(int? limit, int defaultLimit)
    {
        var effectiveLimit = limit ?? defaultLimit;

        if (effectiveLimit < 1)
        {
            return Result<int>.Failure("O limite deve ser maior que zero.");
        }

        if (effectiveLimit > SupplierSearchOptions.MaxLimit)
        {
            return Result<int>.Failure(
                $"O limite máximo permitido é {SupplierSearchOptions.MaxLimit}.");
        }

        return Result<int>.Success(effectiveLimit);
    }

    public static Result<(string Search, int Limit)> ValidateSearch(string? search, int? limit)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return Result<(string Search, int Limit)>.Failure("Informe ao menos 2 caracteres para buscar.");
        }

        var normalizedSearch = search.Trim();

        if (normalizedSearch.Length < SupplierSearchOptions.MinSearchLength)
        {
            return Result<(string Search, int Limit)>.Failure("Informe ao menos 2 caracteres para buscar.");
        }

        var limitResult = ValidateLimit(limit, SupplierSearchOptions.DefaultLimit);
        if (limitResult.IsFailure)
        {
            return Result<(string Search, int Limit)>.Failure(limitResult.Error!);
        }

        return Result<(string Search, int Limit)>.Success((normalizedSearch, limitResult.Value!));
    }
}
