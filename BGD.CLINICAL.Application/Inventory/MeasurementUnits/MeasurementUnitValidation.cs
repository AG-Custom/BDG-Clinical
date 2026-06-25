using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Domain.Enums;

namespace BGD.CLINICAL.Application.Inventory.MeasurementUnits;

internal static class MeasurementUnitSearchOptions
{
    public const int DefaultLimit = 20;
    public const int MaxLimit = 50;
    public const int MinSearchLength = 2;
}

internal static class MeasurementUnitValidation
{
    public static Result<int> ValidateLimit(int? limit, int defaultLimit)
    {
        var effectiveLimit = limit ?? defaultLimit;

        if (effectiveLimit < 1)
        {
            return Result<int>.Failure("O limite deve ser maior que zero.");
        }

        if (effectiveLimit > MeasurementUnitSearchOptions.MaxLimit)
        {
            return Result<int>.Failure(
                $"O limite máximo permitido é {MeasurementUnitSearchOptions.MaxLimit}.");
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

        if (normalizedSearch.Length < MeasurementUnitSearchOptions.MinSearchLength)
        {
            return Result<(string Search, int Limit)>.Failure("Informe ao menos 2 caracteres para buscar.");
        }

        var limitResult = ValidateLimit(limit, MeasurementUnitSearchOptions.DefaultLimit);
        if (limitResult.IsFailure)
        {
            return Result<(string Search, int Limit)>.Failure(limitResult.Error!);
        }

        return Result<(string Search, int Limit)>.Success((normalizedSearch, limitResult.Value!));
    }

    public static string? NormalizeNome(string? nome)
    {
        return string.IsNullOrWhiteSpace(nome) ? null : nome.Trim();
    }

    public static string? NormalizeSigla(string? sigla)
    {
        return string.IsNullOrWhiteSpace(sigla) ? null : sigla.Trim();
    }

    public static Result<TipoUnidadeMedida> ParseTipo(string? tipo)
    {
        if (string.IsNullOrWhiteSpace(tipo))
        {
            return Result<TipoUnidadeMedida>.Failure("Informe o tipo da unidade de medida.");
        }

        if (!Enum.TryParse<TipoUnidadeMedida>(tipo.Trim(), ignoreCase: true, out var parsed)
            || !Enum.IsDefined(typeof(TipoUnidadeMedida), parsed))
        {
            return Result<TipoUnidadeMedida>.Failure(
                "Tipo de unidade de medida inválido. Valores aceitos: Massa, Volume, Unidade, Embalagem, Outro.");
        }

        return Result<TipoUnidadeMedida>.Success(parsed);
    }
}
