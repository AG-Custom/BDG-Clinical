using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Domain.Enums;
using BGD.CLINICAL.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace BGD.CLINICAL.Infra.Data.Repositories.Inventory;

public sealed class StockBalancesRepository : IStockBalancesRepository
{
    private readonly AppDbContext _context;

    public StockBalancesRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<StockBalanceRow>> ListByEmpresaIdAsync(
        Guid empresaId,
        Guid? unidadeId,
        Guid? produtoId,
        bool? abaixoDoMinimo,
        string? search,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var balancesQuery = _context.MovimentacoesEstoque
            .AsNoTracking()
            .Where(movimentacao => movimentacao.EmpresaId == empresaId);

        if (unidadeId.HasValue)
        {
            balancesQuery = balancesQuery.Where(movimentacao => movimentacao.UnidadeId == unidadeId.Value);
        }

        if (produtoId.HasValue)
        {
            balancesQuery = balancesQuery.Where(movimentacao => movimentacao.ProdutoId == produtoId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            balancesQuery = balancesQuery.Where(movimentacao =>
                _context.Produtos.Any(produto =>
                    produto.Id == movimentacao.ProdutoId
                    && produto.EmpresaId == empresaId
                    && EF.Functions.Like(produto.Nome, pattern)));
        }

        var grouped = balancesQuery
            .GroupBy(movimentacao => new { movimentacao.UnidadeId, movimentacao.ProdutoId })
            .Select(group => new
            {
                group.Key.UnidadeId,
                group.Key.ProdutoId,
                SaldoAtual =
                    group.Where(movimentacao => movimentacao.Tipo == TipoMovimentacaoEstoque.Entrada)
                        .Sum(movimentacao => movimentacao.Quantidade)
                    - group.Where(movimentacao => movimentacao.Tipo == TipoMovimentacaoEstoque.Saida)
                        .Sum(movimentacao => movimentacao.Quantidade)
                    + group.Where(movimentacao => movimentacao.Tipo == TipoMovimentacaoEstoque.Ajuste)
                        .Sum(movimentacao => movimentacao.Quantidade)
                    - group.Where(movimentacao => movimentacao.Tipo == TipoMovimentacaoEstoque.Perda)
                        .Sum(movimentacao => movimentacao.Quantidade)
            });

        var query =
            from balance in grouped
            join produto in _context.Produtos.AsNoTracking() on balance.ProdutoId equals produto.Id
            join unidade in _context.Unidades.AsNoTracking() on balance.UnidadeId equals unidade.Id
            join unidadeMedida in _context.UnidadesMedida.AsNoTracking() on produto.UnidadeMedidaId equals unidadeMedida.Id
            where produto.EmpresaId == empresaId
            select new
            {
                balance.UnidadeId,
                UnidadeNome = unidade.Nome,
                balance.ProdutoId,
                ProdutoNome = produto.Nome,
                UnidadeMedidaSigla = unidadeMedida.Sigla,
                produto.EstoqueMinimo,
                balance.SaldoAtual,
                ValorUnitario = _context.ItensPedidoFornecedor
                    .AsNoTracking()
                    .Where(item =>
                        item.ProdutoId == balance.ProdutoId
                        && item.PedidoFornecedor.EmpresaId == empresaId
                        && item.PedidoFornecedor.Status == StatusPedidoFornecedor.RecebidoPelaUnidade)
                    .OrderByDescending(item => item.PedidoFornecedor.DataPedido)
                    .ThenByDescending(item => item.PedidoFornecedor.AtualizadoEm)
                    .Select(item => (decimal?)item.ValorUnitario)
                    .FirstOrDefault()
            };

        if (abaixoDoMinimo == true)
        {
            query = query.Where(row => row.SaldoAtual < row.EstoqueMinimo);
        }

        var orderedQuery = query
            .OrderBy(row => row.ProdutoNome)
            .ThenBy(row => row.UnidadeNome);

        var rows = limit.HasValue
            ? await orderedQuery.Take(limit.Value).ToListAsync(cancellationToken)
            : await orderedQuery.ToListAsync(cancellationToken);

        return rows
            .Select(row => new StockBalanceRow(
                row.UnidadeId,
                row.UnidadeNome,
                row.ProdutoId,
                row.ProdutoNome,
                row.UnidadeMedidaSigla,
                row.EstoqueMinimo,
                row.SaldoAtual,
                row.ValorUnitario))
            .ToList();
    }

    public async Task<decimal> GetSaldoByUnidadeAndProdutoAsync(
        Guid empresaId,
        Guid unidadeId,
        Guid produtoId,
        CancellationToken cancellationToken = default)
    {
        var movimentacoes = _context.MovimentacoesEstoque
            .AsNoTracking()
            .Where(movimentacao =>
                movimentacao.EmpresaId == empresaId
                && movimentacao.UnidadeId == unidadeId
                && movimentacao.ProdutoId == produtoId);

        var entradas = await movimentacoes
            .Where(movimentacao => movimentacao.Tipo == TipoMovimentacaoEstoque.Entrada)
            .SumAsync(movimentacao => movimentacao.Quantidade, cancellationToken);

        var saidas = await movimentacoes
            .Where(movimentacao => movimentacao.Tipo == TipoMovimentacaoEstoque.Saida)
            .SumAsync(movimentacao => movimentacao.Quantidade, cancellationToken);

        var ajustes = await movimentacoes
            .Where(movimentacao => movimentacao.Tipo == TipoMovimentacaoEstoque.Ajuste)
            .SumAsync(movimentacao => movimentacao.Quantidade, cancellationToken);

        var perdas = await movimentacoes
            .Where(movimentacao => movimentacao.Tipo == TipoMovimentacaoEstoque.Perda)
            .SumAsync(movimentacao => movimentacao.Quantidade, cancellationToken);

        return entradas - saidas + ajustes - perdas;
    }
}
