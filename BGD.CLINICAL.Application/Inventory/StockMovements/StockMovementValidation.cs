using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Domain.Enums;

namespace BGD.CLINICAL.Application.Inventory.StockMovements;

internal static class StockMovementListOptions
{
    public const int DefaultLimit = 50;
    public const int MaxLimit = 200;
}

internal static class StockMovementValidation
{
    public static Result<int> ValidateLimit(int? limit)
    {
        var effectiveLimit = limit ?? StockMovementListOptions.DefaultLimit;

        if (effectiveLimit < 1)
        {
            return Result<int>.Failure("O limite deve ser maior que zero.");
        }

        if (effectiveLimit > StockMovementListOptions.MaxLimit)
        {
            return Result<int>.Failure(
                $"O limite máximo permitido é {StockMovementListOptions.MaxLimit}.");
        }

        return Result<int>.Success(effectiveLimit);
    }

    public static Result<TipoMovimentacaoEstoque?> ParseTipo(string? tipo)
    {
        if (string.IsNullOrWhiteSpace(tipo))
        {
            return Result<TipoMovimentacaoEstoque?>.Success(null);
        }

        if (!Enum.TryParse<TipoMovimentacaoEstoque>(tipo.Trim(), ignoreCase: true, out var parsed)
            || !Enum.IsDefined(typeof(TipoMovimentacaoEstoque), parsed))
        {
            return Result<TipoMovimentacaoEstoque?>.Failure(
                "Tipo de movimentação inválido. Valores aceitos: Entrada, Saida, Ajuste, Perda.");
        }

        return Result<TipoMovimentacaoEstoque?>.Success(parsed);
    }

    public static Result<(DateTime? DataInicio, DateTime? DataFim)> ValidateDateRange(
        DateTime? dataInicio,
        DateTime? dataFim)
    {
        if (dataInicio.HasValue && dataFim.HasValue && dataInicio.Value > dataFim.Value)
        {
            return Result<(DateTime? DataInicio, DateTime? DataFim)>.Failure(
                "A data inicial não pode ser maior que a data final.");
        }

        return Result<(DateTime? DataInicio, DateTime? DataFim)>.Success((dataInicio, dataFim));
    }
}
