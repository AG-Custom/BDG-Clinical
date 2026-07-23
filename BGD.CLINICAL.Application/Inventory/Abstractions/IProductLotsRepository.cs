using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Application.Inventory.Abstractions;

public interface IProductLotsRepository
{
    Task<LoteProduto?> GetByCodigoAsync(
        Guid empresaId,
        Guid unidadeId,
        Guid produtoId,
        string codigo,
        CancellationToken cancellationToken = default);

    Task<LoteProduto?> GetByIdAndEmpresaIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default);

    Task AddAsync(LoteProduto lote, CancellationToken cancellationToken = default);
}

public sealed record LotBalanceRow(
    Guid LoteProdutoId,
    Guid UnidadeId,
    string UnidadeNome,
    Guid ProdutoId,
    string ProdutoNome,
    string Codigo,
    DateOnly DataValidade,
    decimal SaldoAtual,
    string UnidadeMedidaSigla,
    decimal? FatorEmbalagemParaEstoque);
