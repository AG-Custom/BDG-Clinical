using BGD.CLINICAL.Application.Applications.Abstractions;
using BGD.CLINICAL.Application.Applications.Dtos;
using BGD.CLINICAL.Application.Common;

namespace BGD.CLINICAL.Application.Applications.Procedures;

internal sealed record ValidatedProcedureData(
    string Nome,
    Guid? ProdutoAplicadoId,
    string? Observacoes,
    IReadOnlyList<(Guid ProdutoId, decimal Quantidade)> Itens);

internal static class ProcedureSearchOptions
{
    public const int DefaultLimit = 20;
    public const int MaxLimit = 50;
    public const int MinSearchLength = 2;
}

internal static class ProcedureRequestValidator
{
    public static async Task<Result<ValidatedProcedureData>> ValidateAsync(
        Guid empresaId,
        string? nome,
        Guid? produtoAplicadoId,
        string? observacoes,
        IReadOnlyList<CreateProcedureItemRequest>? itens,
        Guid? excludeProcedureId,
        IProceduresRepository proceduresRepository,
        CancellationToken cancellationToken)
    {
        var normalizedNome = string.IsNullOrWhiteSpace(nome) ? null : nome.Trim();
        if (normalizedNome is null)
        {
            return Result<ValidatedProcedureData>.Failure("Informe o nome do procedimento.");
        }

        if (normalizedNome.Length > 200)
        {
            return Result<ValidatedProcedureData>.Failure(
                "O nome do procedimento deve ter no máximo 200 caracteres.");
        }

        if (produtoAplicadoId.HasValue && produtoAplicadoId.Value == Guid.Empty)
        {
            return Result<ValidatedProcedureData>.Failure("Produto aplicado inválido.");
        }

        if (!string.IsNullOrWhiteSpace(observacoes) && observacoes.Length > 2000)
        {
            return Result<ValidatedProcedureData>.Failure(
                "As observações devem ter no máximo 2000 caracteres.");
        }

        var normalizedItens = (itens ?? []).ToList();

        if (!produtoAplicadoId.HasValue && normalizedItens.Count == 0)
        {
            return Result<ValidatedProcedureData>.Failure(
                "Informe o produto aplicado ou ao menos um item consumido.");
        }

        if (normalizedItens.Count > 0)
        {
            if (normalizedItens.Any(item => item.ProdutoId == Guid.Empty))
            {
                return Result<ValidatedProcedureData>.Failure(
                    "Informe o produto de cada item do procedimento.");
            }

            if (normalizedItens.Any(item => item.Quantidade <= 0))
            {
                return Result<ValidatedProcedureData>.Failure(
                    "A quantidade de cada item deve ser maior que zero.");
            }

            if (normalizedItens.GroupBy(item => item.ProdutoId).Any(group => group.Count() > 1))
            {
                return Result<ValidatedProcedureData>.Failure(
                    "Não é permitido repetir o mesmo produto nos itens do procedimento.");
            }

            if (produtoAplicadoId.HasValue
                && normalizedItens.Any(item => item.ProdutoId == produtoAplicadoId.Value))
            {
                return Result<ValidatedProcedureData>.Failure(
                    "O produto aplicado não pode constar na lista de itens consumidos.");
            }
        }

        var productIds = normalizedItens
            .Select(item => item.ProdutoId)
            .ToList();

        if (produtoAplicadoId.HasValue)
        {
            productIds.Add(produtoAplicadoId.Value);
        }

        if (productIds.Count > 0)
        {
            var productsExist = await proceduresRepository.AllActiveProductsExistAsync(
                empresaId,
                productIds,
                cancellationToken);

            if (!productsExist)
            {
                return Result<ValidatedProcedureData>.Failure(
                    "Um ou mais produtos informados não foram encontrados ou estão inativos.");
            }
        }

        if (await proceduresRepository.ExistsByNomeAsync(
                empresaId,
                normalizedNome,
                excludeProcedureId,
                cancellationToken))
        {
            return Result<ValidatedProcedureData>.Failure("Já existe um procedimento com este nome.");
        }

        var mappedItens = normalizedItens
            .Select(item => (item.ProdutoId, item.Quantidade))
            .ToList();

        return Result<ValidatedProcedureData>.Success(new ValidatedProcedureData(
            normalizedNome,
            produtoAplicadoId,
            string.IsNullOrWhiteSpace(observacoes) ? null : observacoes.Trim(),
            mappedItens));
    }

    public static Result<int> ValidateLimit(int? limit, int defaultLimit)
    {
        var effectiveLimit = limit ?? defaultLimit;

        if (effectiveLimit < 1)
        {
            return Result<int>.Failure("O limite deve ser maior que zero.");
        }

        if (effectiveLimit > ProcedureSearchOptions.MaxLimit)
        {
            return Result<int>.Failure(
                $"O limite máximo permitido é {ProcedureSearchOptions.MaxLimit}.");
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

        if (normalizedSearch.Length < ProcedureSearchOptions.MinSearchLength)
        {
            return Result<(string Search, int Limit)>.Failure("Informe ao menos 2 caracteres para buscar.");
        }

        var limitResult = ValidateLimit(limit, ProcedureSearchOptions.DefaultLimit);
        if (limitResult.IsFailure)
        {
            return Result<(string Search, int Limit)>.Failure(limitResult.Error!);
        }

        return Result<(string Search, int Limit)>.Success((normalizedSearch, limitResult.Value!));
    }
}
