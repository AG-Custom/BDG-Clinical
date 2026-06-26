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
        string? sku,
        string? codigoInterno,
        string? codigoBarras,
        bool controlaEstoque,
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

        var skuTrimmed = string.IsNullOrWhiteSpace(sku) ? null : sku.Trim();
        var codigoInternoTrimmed = string.IsNullOrWhiteSpace(codigoInterno) ? null : codigoInterno.Trim();
        var codigoBarrasTrimmed = string.IsNullOrWhiteSpace(codigoBarras) ? null : codigoBarras.Trim();

        if (skuTrimmed?.Length > 50)
        {
            return Result<ValidatedProductData>.Failure("O SKU deve ter no máximo 50 caracteres.");
        }

        if (codigoInternoTrimmed?.Length > 50)
        {
            return Result<ValidatedProductData>.Failure("O código interno deve ter no máximo 50 caracteres.");
        }

        if (codigoBarrasTrimmed?.Length > 50)
        {
            return Result<ValidatedProductData>.Failure("O código de barras deve ter no máximo 50 caracteres.");
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

        if (skuTrimmed is not null
            && await productsRepository.ExistsBySkuAsync(empresaId, skuTrimmed, excludeProductId, cancellationToken))
        {
            return Result<ValidatedProductData>.Failure("Já existe um produto com este SKU.");
        }

        if (codigoInternoTrimmed is not null
            && await productsRepository.ExistsByCodigoInternoAsync(empresaId, codigoInternoTrimmed, excludeProductId, cancellationToken))
        {
            return Result<ValidatedProductData>.Failure("Já existe um produto com este código interno.");
        }

        return Result<ValidatedProductData>.Success(new ValidatedProductData(
            tipoProdutoId,
            unidadeMedidaId,
            nomeTrimmed,
            estoqueMinimo,
            skuTrimmed,
            codigoInternoTrimmed,
            codigoBarrasTrimmed,
            controlaEstoque));
    }
}

internal sealed record ValidatedProductData(
    Guid TipoProdutoId,
    Guid UnidadeMedidaId,
    string Nome,
    decimal EstoqueMinimo,
    string? Sku,
    string? CodigoInterno,
    string? CodigoBarras,
    bool ControlaEstoque);
