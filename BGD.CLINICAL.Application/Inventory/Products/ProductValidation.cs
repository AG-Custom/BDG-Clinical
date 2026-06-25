using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Application.Inventory.Dtos;

namespace BGD.CLINICAL.Application.Inventory.Products;

internal static class ProductValidation
{
    public static string? ValidateEstoqueMinimo(decimal estoqueMinimo)
    {
        if (estoqueMinimo < 0)
        {
            return "O estoque mínimo não pode ser negativo.";
        }

        return null;
    }
}

internal static class ProductRequestValidator
{
    public static async Task<Result<ValidatedProductData>> ValidateAsync(
        Guid empresaId,
        Guid tipoProdutoId,
        Guid unidadeMedidaId,
        string nome,
        decimal estoqueMinimo,
        Guid? excludeProductId,
        IProductsRepository productsRepository,
        CancellationToken cancellationToken)
    {
        if (tipoProdutoId == Guid.Empty)
        {
            return Result<ValidatedProductData>.Failure("Informe o tipo do produto.");
        }

        if (unidadeMedidaId == Guid.Empty)
        {
            return Result<ValidatedProductData>.Failure("Informe a unidade de medida do produto.");
        }

        if (string.IsNullOrWhiteSpace(nome))
        {
            return Result<ValidatedProductData>.Failure("Informe o nome do produto.");
        }

        var estoqueError = ProductValidation.ValidateEstoqueMinimo(estoqueMinimo);
        if (estoqueError is not null)
        {
            return Result<ValidatedProductData>.Failure(estoqueError);
        }

        if (!await productsRepository.ExistsActiveTipoProdutoInEmpresaAsync(tipoProdutoId, empresaId, cancellationToken))
        {
            return Result<ValidatedProductData>.Failure("Tipo de produto não encontrado.");
        }

        if (!await productsRepository.ExistsActiveUnidadeMedidaInEmpresaAsync(unidadeMedidaId, empresaId, cancellationToken))
        {
            return Result<ValidatedProductData>.Failure("Unidade de medida não encontrada.");
        }

        var nomeTrimmed = nome.Trim();
        if (await productsRepository.ExistsByNomeAsync(empresaId, nomeTrimmed, excludeProductId, cancellationToken))
        {
            return Result<ValidatedProductData>.Failure("Já existe um produto com este nome.");
        }

        return Result<ValidatedProductData>.Success(new ValidatedProductData(
            tipoProdutoId,
            unidadeMedidaId,
            nomeTrimmed,
            estoqueMinimo));
    }
}

internal sealed record ValidatedProductData(
    Guid TipoProdutoId,
    Guid UnidadeMedidaId,
    string Nome,
    decimal EstoqueMinimo);
