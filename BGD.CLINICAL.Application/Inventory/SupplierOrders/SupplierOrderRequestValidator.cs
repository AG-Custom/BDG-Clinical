using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Core.Abstractions;
using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Application.Inventory.Dtos;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using BGD.CLINICAL.Domain.Exceptions;

namespace BGD.CLINICAL.Application.Inventory.SupplierOrders;

internal static class SupplierOrderValidation
{
    public static Result<TipoPedidoFornecedor> ParseTipoPedido(string? tipoPedido)
    {
        if (string.IsNullOrWhiteSpace(tipoPedido))
        {
            return Result<TipoPedidoFornecedor>.Failure("Informe o tipo do pedido.");
        }

        if (!Enum.TryParse<TipoPedidoFornecedor>(tipoPedido.Trim(), ignoreCase: true, out var parsed)
            || !Enum.IsDefined(typeof(TipoPedidoFornecedor), parsed))
        {
            return Result<TipoPedidoFornecedor>.Failure(
                "Tipo de pedido inválido. Valores aceitos: Medicamento, Insumo, Implante, Outro.");
        }

        return Result<TipoPedidoFornecedor>.Success(parsed);
    }

    public static Result<StatusPedidoFornecedor> ParseEditableStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return Result<StatusPedidoFornecedor>.Failure("Informe o status do pedido.");
        }

        if (!StatusPedidoFornecedorExtensions.TryParseFromApi(status, out var parsed))
        {
            return Result<StatusPedidoFornecedor>.Failure(
                $"Status de pedido inválido. Valores aceitos: {StatusPedidoFornecedorExtensions.AllStatusApiList}.");
        }

        if (!StatusPedidoFornecedorExtensions.IsEditable(parsed))
        {
            return Result<StatusPedidoFornecedor>.Failure(
                $"O status do pedido deve ser {StatusPedidoFornecedorExtensions.EditableStatusApiList}.");
        }

        return Result<StatusPedidoFornecedor>.Success(parsed);
    }

    public static Result<StatusPedidoFornecedor?> ParseListStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return Result<StatusPedidoFornecedor?>.Success(null);
        }

        if (!StatusPedidoFornecedorExtensions.TryParseFromApi(status, out var parsed))
        {
            return Result<StatusPedidoFornecedor?>.Failure(
                $"Status de pedido inválido. Valores aceitos: {StatusPedidoFornecedorExtensions.AllStatusApiList}.");
        }

        return Result<StatusPedidoFornecedor?>.Success(parsed);
    }
}

internal sealed record ValidatedSupplierOrderData(
    Guid FornecedorId,
    Guid UnidadeId,
    TipoPedidoFornecedor TipoPedido,
    DateTime DataPedido,
    StatusPedidoFornecedor Status,
    string? Observacao,
    IReadOnlyList<ItemPedidoFornecedor> Itens);

internal static class SupplierOrderRequestValidator
{
    public static async Task<Result<ValidatedSupplierOrderData>> ValidateAsync(
        Guid empresaId,
        Guid fornecedorId,
        Guid unidadeId,
        string? tipoPedido,
        DateTime dataPedido,
        string? status,
        string? observacao,
        IReadOnlyList<SupplierOrderItemRequest> itens,
        ISuppliersRepository suppliersRepository,
        IUnitsRepository unitsRepository,
        IProductsRepository productsRepository,
        CancellationToken cancellationToken)
    {
        if (fornecedorId == Guid.Empty)
        {
            return Result<ValidatedSupplierOrderData>.Failure("Informe o fornecedor do pedido.");
        }

        if (unidadeId == Guid.Empty)
        {
            return Result<ValidatedSupplierOrderData>.Failure("Informe a unidade do pedido.");
        }

        if (itens.Count == 0)
        {
            return Result<ValidatedSupplierOrderData>.Failure("Informe ao menos um item no pedido.");
        }

        var tipoResult = SupplierOrderValidation.ParseTipoPedido(tipoPedido);
        if (tipoResult.IsFailure)
        {
            return Result<ValidatedSupplierOrderData>.Failure(tipoResult.Error!);
        }

        var statusResult = SupplierOrderValidation.ParseEditableStatus(status);
        if (statusResult.IsFailure)
        {
            return Result<ValidatedSupplierOrderData>.Failure(statusResult.Error!);
        }

        if (!await suppliersRepository.ExistsActiveByIdAndEmpresaIdAsync(fornecedorId, empresaId, cancellationToken))
        {
            return Result<ValidatedSupplierOrderData>.Failure("Fornecedor não encontrado.");
        }

        var unidade = await unitsRepository.GetByIdAndEmpresaIdAsync(unidadeId, empresaId, cancellationToken);
        if (unidade is null || !unidade.Ativo)
        {
            return Result<ValidatedSupplierOrderData>.Failure("Unidade não encontrada.");
        }

        var domainItens = new List<ItemPedidoFornecedor>();
        var produtoIds = new HashSet<Guid>();

        foreach (var item in itens)
        {
            if (item.ProdutoId == Guid.Empty)
            {
                return Result<ValidatedSupplierOrderData>.Failure("Informe o produto do item.");
            }

            if (!produtoIds.Add(item.ProdutoId))
            {
                return Result<ValidatedSupplierOrderData>.Failure("Não é permitido repetir o mesmo produto no pedido.");
            }

            if (!await productsRepository.ExistsActiveByIdAndEmpresaIdAsync(item.ProdutoId, empresaId, cancellationToken))
            {
                return Result<ValidatedSupplierOrderData>.Failure("Produto não encontrado.");
            }

            try
            {
                domainItens.Add(ItemPedidoFornecedor.CreateForNewPedido(
                    item.ProdutoId,
                    item.Quantidade,
                    item.ValorUnitario,
                    item.ValorTotal));
            }
            catch (DomainException exception)
            {
                return Result<ValidatedSupplierOrderData>.Failure(exception.Message);
            }
        }

        return Result<ValidatedSupplierOrderData>.Success(new ValidatedSupplierOrderData(
            fornecedorId,
            unidadeId,
            tipoResult.Value!,
            dataPedido,
            statusResult.Value!,
            string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim(),
            domainItens));
    }
}
