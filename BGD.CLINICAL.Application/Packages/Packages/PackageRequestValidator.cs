using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Application.Packages.Abstractions;
using BGD.CLINICAL.Application.Packages.Dtos;

namespace BGD.CLINICAL.Application.Packages.Packages;

internal sealed record ValidatedPackageData(
    string Nome,
    string? Descricao,
    decimal Valor,
    IReadOnlyList<(Guid ProdutoId, decimal QuantidadeTotal, string UnidadeMedida)> Itens);

internal static class PackageSearchOptions
{
    public const int DefaultLimit = 20;
    public const int MaxLimit = 50;
    public const int MinSearchLength = 2;
}

internal static class PackageRequestValidator
{
    public static async Task<Result<ValidatedPackageData>> ValidateAsync(
        Guid empresaId,
        string? nome,
        string? descricao,
        decimal valor,
        IReadOnlyList<CreatePackageItemRequest>? itens,
        Guid? excludePackageId,
        IPackagesRepository packagesRepository,
        IProductsRepository productsRepository,
        CancellationToken cancellationToken)
    {
        var normalizedNome = string.IsNullOrWhiteSpace(nome) ? null : nome.Trim();
        if (normalizedNome is null)
        {
            return Result<ValidatedPackageData>.Failure("Informe o nome do pacote.");
        }

        if (normalizedNome.Length > 160)
        {
            return Result<ValidatedPackageData>.Failure("O nome do pacote deve ter no máximo 160 caracteres.");
        }

        if (!string.IsNullOrWhiteSpace(descricao) && descricao.Length > 1000)
        {
            return Result<ValidatedPackageData>.Failure("A descrição deve ter no máximo 1000 caracteres.");
        }

        if (valor < 0)
        {
            return Result<ValidatedPackageData>.Failure("O valor do pacote não pode ser negativo.");
        }

        var normalizedItens = (itens ?? []).ToList();
        if (normalizedItens.Count == 0)
        {
            return Result<ValidatedPackageData>.Failure("Informe ao menos um item para o pacote.");
        }

        if (normalizedItens.Any(item => item.ProdutoId == Guid.Empty))
        {
            return Result<ValidatedPackageData>.Failure("Informe o produto de cada item do pacote.");
        }

        if (normalizedItens.Any(item => item.QuantidadeTotal <= 0))
        {
            return Result<ValidatedPackageData>.Failure("A quantidade de cada item deve ser maior que zero.");
        }

        if (normalizedItens.Any(item => string.IsNullOrWhiteSpace(item.UnidadeMedida)))
        {
            return Result<ValidatedPackageData>.Failure("Informe a unidade de medida de cada item do pacote.");
        }

        if (normalizedItens.Any(item => item.UnidadeMedida.Trim().Length > 30))
        {
            return Result<ValidatedPackageData>.Failure("A unidade de medida deve ter no máximo 30 caracteres.");
        }

        if (normalizedItens.GroupBy(item => item.ProdutoId).Any(group => group.Count() > 1))
        {
            return Result<ValidatedPackageData>.Failure("Não é permitido repetir o mesmo produto nos itens do pacote.");
        }

        var productIds = normalizedItens.Select(item => item.ProdutoId).Distinct().ToList();
        foreach (var productId in productIds)
        {
            var exists = await productsRepository.ExistsActiveByIdAndEmpresaIdAsync(
                productId,
                empresaId,
                cancellationToken);

            if (!exists)
            {
                return Result<ValidatedPackageData>.Failure("Um ou mais produtos do pacote são inválidos ou inativos.");
            }
        }

        if (await packagesRepository.ExistsByNomeAsync(empresaId, normalizedNome, excludePackageId, cancellationToken))
        {
            return Result<ValidatedPackageData>.Failure("Já existe um pacote com este nome.");
        }

        return Result<ValidatedPackageData>.Success(new ValidatedPackageData(
            normalizedNome,
            string.IsNullOrWhiteSpace(descricao) ? null : descricao.Trim(),
            valor,
            normalizedItens
                .Select(item => (item.ProdutoId, item.QuantidadeTotal, item.UnidadeMedida.Trim()))
                .ToList()));
    }

    public static Result<(string Search, int Limit)> ValidateSearch(string search, int? limit)
    {
        var normalized = search.Trim();
        if (normalized.Length < PackageSearchOptions.MinSearchLength)
        {
            return Result<(string, int)>.Failure(
                $"A busca deve ter no mínimo {PackageSearchOptions.MinSearchLength} caracteres.");
        }

        var limitResult = ValidateLimit(limit, PackageSearchOptions.DefaultLimit);
        if (limitResult.IsFailure)
        {
            return Result<(string, int)>.Failure(limitResult.Error!);
        }

        return Result<(string, int)>.Success((normalized, limitResult.Value!));
    }

    public static Result<int> ValidateLimit(int? limit, int defaultLimit)
    {
        var value = limit ?? defaultLimit;
        if (value <= 0 || value > PackageSearchOptions.MaxLimit)
        {
            return Result<int>.Failure($"O limite deve estar entre 1 e {PackageSearchOptions.MaxLimit}.");
        }

        return Result<int>.Success(value);
    }
}
