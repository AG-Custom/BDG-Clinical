using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;

namespace BGD.CLINICAL.Application.Inventory.Abstractions;

public interface IStockMovementsRepository
{
    Task AddAsync(
        MovimentacaoEstoque movimentacao,
        CancellationToken cancellationToken = default);

    Task AddRangeAsync(
        IReadOnlyList<MovimentacaoEstoque> movimentacoes,
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
}
