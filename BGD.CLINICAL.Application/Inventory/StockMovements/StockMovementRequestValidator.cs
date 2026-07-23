using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Core.Abstractions;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Application.Inventory.Dtos;

namespace BGD.CLINICAL.Application.Inventory.StockMovements;

internal sealed record ValidatedManualStockMovementData(
    Guid UnidadeId,
    Guid ProdutoId,
    DateTime Data,
    string? Observacao,
    Guid? FuncionarioId);

internal static class StockMovementRequestValidator
{
    public static async Task<Result<ValidatedManualStockMovementData>> ValidateManualAsync(
        Guid empresaId,
        Guid usuarioId,
        CreateManualStockMovementRequest request,
        bool requireAvailableBalance,
        bool requiresLotForEntry,
        IUnitsRepository unitsRepository,
        IProductsRepository productsRepository,
        IStockBalancesRepository stockBalancesRepository,
        IUsersRepository usersRepository,
        CancellationToken cancellationToken)
    {
        if (request.UnidadeId == Guid.Empty)
        {
            return Result<ValidatedManualStockMovementData>.Failure("Informe a unidade.");
        }

        if (request.ProdutoId == Guid.Empty)
        {
            return Result<ValidatedManualStockMovementData>.Failure("Informe o produto.");
        }

        if (!string.IsNullOrWhiteSpace(request.Observacao) && request.Observacao.Length > 2000)
        {
            return Result<ValidatedManualStockMovementData>.Failure("A observação deve ter no máximo 2000 caracteres.");
        }

        if (requiresLotForEntry)
        {
            if (request.QuantidadeEmbalagem is null or <= 0)
            {
                return Result<ValidatedManualStockMovementData>.Failure(
                    "Informe a quantidade de embalagens maior que zero.");
            }

            if (string.IsNullOrWhiteSpace(request.LoteCodigo))
            {
                return Result<ValidatedManualStockMovementData>.Failure("Informe o código do lote.");
            }

            if (request.DataValidade is null)
            {
                return Result<ValidatedManualStockMovementData>.Failure("Informe a data de validade do lote.");
            }
        }
        else if (request.Quantidade is null or <= 0)
        {
            return Result<ValidatedManualStockMovementData>.Failure("A quantidade deve ser maior que zero.");
        }

        var unidade = await unitsRepository.GetByIdAndEmpresaIdAsync(
            request.UnidadeId,
            empresaId,
            cancellationToken);

        if (unidade is null)
        {
            return Result<ValidatedManualStockMovementData>.Failure("Unidade não encontrada.");
        }

        if (!unidade.Ativo)
        {
            return Result<ValidatedManualStockMovementData>.Failure("A unidade está inativa.");
        }

        var produtoExists = await productsRepository.ExistsActiveByIdAndEmpresaIdAsync(
            request.ProdutoId,
            empresaId,
            cancellationToken);

        if (!produtoExists)
        {
            return Result<ValidatedManualStockMovementData>.Failure("Produto não encontrado ou inativo.");
        }

        if (requireAvailableBalance && request.Quantidade is > 0)
        {
            var saldo = await stockBalancesRepository.GetSaldoByUnidadeAndProdutoAsync(
                empresaId,
                request.UnidadeId,
                request.ProdutoId,
                cancellationToken);

            if (saldo < request.Quantidade.Value)
            {
                return Result<ValidatedManualStockMovementData>.Failure(
                    "Estoque insuficiente na unidade para a quantidade informada.");
            }
        }

        var usuario = await usersRepository.GetByIdAsync(usuarioId, cancellationToken);
        var funcionarioId = usuario?.FuncionarioId;

        return Result<ValidatedManualStockMovementData>.Success(new ValidatedManualStockMovementData(
            request.UnidadeId,
            request.ProdutoId,
            request.Data,
            string.IsNullOrWhiteSpace(request.Observacao) ? null : request.Observacao.Trim(),
            funcionarioId));
    }
}
