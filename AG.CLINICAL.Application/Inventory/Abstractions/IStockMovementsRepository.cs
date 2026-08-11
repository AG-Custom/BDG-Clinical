using AG.CLINICAL.Domain.Entities;
using AG.CLINICAL.Domain.Enums;

namespace AG.CLINICAL.Application.Inventory.Abstractions;

public sealed record StockTransferBalanceRequirement(
    Guid UnidadeId,
    Guid ProdutoId,
    Guid? LoteProdutoId,
    decimal Quantidade);

public interface IStockMovementsRepository
{
    Task AddAsync(
        MovimentacaoEstoque movimentacao,
        CancellationToken cancellationToken = default);

    Task AddRangeAsync(
        IReadOnlyList<MovimentacaoEstoque> movimentacoes,
        CancellationToken cancellationToken = default);

    Task AddTransferAtomicallyAsync(
        Guid empresaId,
        IReadOnlyList<MovimentacaoEstoque> movimentacoes,
        IReadOnlyList<StockTransferBalanceRequirement> saldosNecessarios,
        CancellationToken cancellationToken = default);

    Task<MovimentacaoEstoque?> GetByIdAndEmpresaIdWithDetailsAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MovimentacaoEstoque>> ListByEmpresaIdAsync(
        Guid empresaId,
        Guid? unidadeId,
        Guid? produtoId,
        TipoMovimentacaoEstoque? tipo,
        DateTime? dataInicio,
        DateTime? dataFim,
        int limit,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<(Guid PedidoId, Guid ProdutoId), decimal>> GetValoresUnitariosPorPedidosAsync(
        Guid empresaId,
        IReadOnlyCollection<Guid> pedidoIds,
        CancellationToken cancellationToken = default);
}
