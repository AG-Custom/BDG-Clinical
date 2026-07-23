using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Application.Inventory.Dtos;
using BGD.CLINICAL.Domain.Constants;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Exceptions;

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

    public static string? ValidateValor(decimal valor)
    {
        if (valor < 0)
        {
            return "O valor do produto não pode ser negativo.";
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
        decimal valor,
        string? sku,
        string? codigoInterno,
        string? codigoBarras,
        bool controlaEstoque,
        Guid? unidadeEmbalagemId,
        decimal? conteudoPorEmbalagem,
        Guid? unidadeConteudoId,
        decimal? concentracaoPorConteudo,
        Guid? excludeProductId,
        IProductsRepository productsRepository,
        IProductTypesRepository productTypesRepository,
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

        var valorError = ProductValidation.ValidateValor(valor);
        if (valorError is not null)
        {
            return Result<ValidatedProductData>.Failure(valorError);
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

        var tipoProduto = await productTypesRepository.GetByIdAndEmpresaIdAsync(
            tipoProdutoId,
            empresaId,
            cancellationToken);

        if (tipoProduto is null || !tipoProduto.Ativo)
        {
            return Result<ValidatedProductData>.Failure("Tipo de produto não encontrado.");
        }

        if (!await productsRepository.ExistsActiveUnidadeMedidaInEmpresaAsync(unidadeMedidaId, empresaId, cancellationToken))
        {
            return Result<ValidatedProductData>.Failure("Unidade de medida não encontrada.");
        }

        var isMedicamento = tipoProduto.Codigo == ProductTypeCodes.Medicamento;
        Guid? embalagemId = null;
        Guid? conteudoId = null;
        decimal? conteudoPorEmb = null;
        decimal? concentracao = null;

        if (isMedicamento)
        {
            try
            {
                Produto.ValidateConversaoMedicamentoObrigatoria(
                    unidadeEmbalagemId,
                    conteudoPorEmbalagem,
                    unidadeConteudoId,
                    concentracaoPorConteudo);
            }
            catch (DomainException exception)
            {
                return Result<ValidatedProductData>.Failure(exception.Message);
            }

            embalagemId = unidadeEmbalagemId;
            conteudoId = unidadeConteudoId;
            conteudoPorEmb = conteudoPorEmbalagem;
            concentracao = concentracaoPorConteudo;

            if (!await productsRepository.ExistsActiveUnidadeMedidaInEmpresaAsync(
                    embalagemId!.Value,
                    empresaId,
                    cancellationToken))
            {
                return Result<ValidatedProductData>.Failure("Unidade de embalagem não encontrada.");
            }

            if (!await productsRepository.ExistsActiveUnidadeMedidaInEmpresaAsync(
                    conteudoId!.Value,
                    empresaId,
                    cancellationToken))
            {
                return Result<ValidatedProductData>.Failure("Unidade de conteúdo não encontrada.");
            }
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
            valor,
            skuTrimmed,
            codigoInternoTrimmed,
            codigoBarrasTrimmed,
            controlaEstoque,
            embalagemId,
            conteudoPorEmb,
            conteudoId,
            concentracao));
    }
}

internal sealed record ValidatedProductData(
    Guid TipoProdutoId,
    Guid UnidadeMedidaId,
    string Nome,
    decimal EstoqueMinimo,
    decimal Valor,
    string? Sku,
    string? CodigoInterno,
    string? CodigoBarras,
    bool ControlaEstoque,
    Guid? UnidadeEmbalagemId,
    decimal? ConteudoPorEmbalagem,
    Guid? UnidadeConteudoId,
    decimal? ConcentracaoPorConteudo);
