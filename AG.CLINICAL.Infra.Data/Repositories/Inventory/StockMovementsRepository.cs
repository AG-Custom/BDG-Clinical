using AG.CLINICAL.Application.Inventory.Abstractions;
using AG.CLINICAL.Domain.Entities;
using AG.CLINICAL.Domain.Enums;
using AG.CLINICAL.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Data;
using AG.CLINICAL.Domain.Exceptions;

namespace AG.CLINICAL.Infra.Data.Repositories.Inventory;

public sealed class StockMovementsRepository : IStockMovementsRepository
{
    private readonly AppDbContext _context;

    public StockMovementsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        MovimentacaoEstoque movimentacao,
        CancellationToken cancellationToken = default)
    {
        await _context.MovimentacoesEstoque.AddAsync(movimentacao, cancellationToken);
    }

    public async Task AddRangeAsync(
        IReadOnlyList<MovimentacaoEstoque> movimentacoes,
        CancellationToken cancellationToken = default)
    {
        await _context.MovimentacoesEstoque.AddRangeAsync(movimentacoes, cancellationToken);
    }

    public async Task AddTransferAtomicallyAsync(
        Guid empresaId,
        IReadOnlyList<MovimentacaoEstoque> movimentacoes,
        IReadOnlyList<StockTransferBalanceRequirement> saldosNecessarios,
        CancellationToken cancellationToken = default)
    {
        var executionStrategy = _context.Database.CreateExecutionStrategy();

        await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            try
            {
                foreach (var requisito in saldosNecessarios)
                {
                    var query = _context.MovimentacoesEstoque
                        .Where(movimentacao =>
                            movimentacao.EmpresaId == empresaId
                            && movimentacao.UnidadeId == requisito.UnidadeId
                            && movimentacao.ProdutoId == requisito.ProdutoId);

                    if (requisito.LoteProdutoId.HasValue)
                    {
                        query = query.Where(movimentacao =>
                            movimentacao.LoteProdutoId == requisito.LoteProdutoId.Value);
                    }

                    var saldo = await query.SumAsync(
                        movimentacao =>
                            movimentacao.Tipo == TipoMovimentacaoEstoque.Entrada
                            || movimentacao.Tipo == TipoMovimentacaoEstoque.Ajuste
                                ? movimentacao.Quantidade
                                : -movimentacao.Quantidade,
                        cancellationToken);

                    if (saldo < requisito.Quantidade)
                    {
                        var escopo = requisito.LoteProdutoId.HasValue ? " no lote selecionado" : string.Empty;
                        throw new DomainException(
                            $"Saldo insuficiente{escopo}. Saldo disponível: {saldo}; quantidade solicitada: {requisito.Quantidade}.");
                    }
                }

                await _context.MovimentacoesEstoque.AddRangeAsync(movimentacoes, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    public async Task<IReadOnlyList<MovimentacaoEstoque>> ListByEmpresaIdAsync(
        Guid empresaId,
        Guid? unidadeId,
        Guid? produtoId,
        TipoMovimentacaoEstoque? tipo,
        DateTime? dataInicio,
        DateTime? dataFim,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var query = _context.MovimentacoesEstoque
            .AsNoTracking()
            .Include(movimentacao => movimentacao.Unidade)
            .Include(movimentacao => movimentacao.Produto)
            .Include(movimentacao => movimentacao.LoteProduto)
            .Where(movimentacao => movimentacao.EmpresaId == empresaId);

        if (unidadeId.HasValue)
        {
            query = query.Where(movimentacao => movimentacao.UnidadeId == unidadeId.Value);
        }

        if (produtoId.HasValue)
        {
            query = query.Where(movimentacao => movimentacao.ProdutoId == produtoId.Value);
        }

        if (tipo.HasValue)
        {
            query = query.Where(movimentacao => movimentacao.Tipo == tipo.Value);
        }

        if (dataInicio.HasValue)
        {
            query = query.Where(movimentacao => movimentacao.Data >= dataInicio.Value);
        }

        if (dataFim.HasValue)
        {
            query = query.Where(movimentacao => movimentacao.Data <= dataFim.Value);
        }

        return await query
            .OrderByDescending(movimentacao => movimentacao.Data)
            .ThenByDescending(movimentacao => movimentacao.CriadoEm)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public Task<MovimentacaoEstoque?> GetByIdAndEmpresaIdWithDetailsAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return _context.MovimentacoesEstoque
            .Include(movimentacao => movimentacao.Unidade)
            .Include(movimentacao => movimentacao.Produto)
            .Include(movimentacao => movimentacao.LoteProduto)
            .FirstOrDefaultAsync(
                movimentacao => movimentacao.Id == id && movimentacao.EmpresaId == empresaId,
                cancellationToken);
    }

    public async Task<IReadOnlyDictionary<(Guid PedidoId, Guid ProdutoId), decimal>> GetValoresUnitariosPorPedidosAsync(
        Guid empresaId,
        IReadOnlyCollection<Guid> pedidoIds,
        CancellationToken cancellationToken = default)
    {
        if (pedidoIds.Count == 0)
        {
            return new Dictionary<(Guid PedidoId, Guid ProdutoId), decimal>();
        }

        var itens = await _context.ItensPedidoFornecedor
            .AsNoTracking()
            .Where(item =>
                pedidoIds.Contains(item.PedidoFornecedorId)
                && item.PedidoFornecedor.EmpresaId == empresaId)
            .Select(item => new
            {
                item.PedidoFornecedorId,
                item.ProdutoId,
                item.ValorUnitario
            })
            .ToListAsync(cancellationToken);

        return itens.ToDictionary(
            item => (item.PedidoFornecedorId, item.ProdutoId),
            item => item.ValorUnitario);
    }
}
