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
                ValorUnitario =
                    _context.ItensPedidoFornecedor
                        .AsNoTracking()
                        .Where(item =>
                            item.ProdutoId == balance.ProdutoId
                            && item.PedidoFornecedor.EmpresaId == empresaId
                            && item.PedidoFornecedor.Status == StatusPedidoFornecedor.RecebidoPelaUnidade)
                        .OrderByDescending(item => item.PedidoFornecedor.DataPedido)
                        .ThenByDescending(item => item.PedidoFornecedor.AtualizadoEm)
                        .Select(item => (decimal?)item.ValorUnitario)
                        .FirstOrDefault()
                    ?? produto.Valor
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

        if (rows.Count == 0)
        {
            return Array.Empty<StockBalanceRow>();
        }

        var unidadeIds = rows.Select(row => row.UnidadeId).Distinct().ToList();
        var produtoIds = rows.Select(row => row.ProdutoId).Distinct().ToList();

        var origensEntrada = await _context.MovimentacoesEstoque
            .AsNoTracking()
            .Where(movimentacao =>
                movimentacao.EmpresaId == empresaId
                && unidadeIds.Contains(movimentacao.UnidadeId)
                && produtoIds.Contains(movimentacao.ProdutoId)
                && movimentacao.Tipo == TipoMovimentacaoEstoque.Entrada)
            .Select(movimentacao => new
            {
                movimentacao.UnidadeId,
                movimentacao.ProdutoId,
                movimentacao.Origem
            })
            .Distinct()
            .ToListAsync(cancellationToken);

        var origensPorChave = origensEntrada
            .GroupBy(item => (item.UnidadeId, item.ProdutoId))
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<string>)group
                    .Select(item => item.Origem)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(origem => origem, StringComparer.Ordinal)
                    .ToList());

        return rows
            .Select(row => new StockBalanceRow(
                row.UnidadeId,
                row.UnidadeNome,
                row.ProdutoId,
                row.ProdutoNome,
                row.UnidadeMedidaSigla,
                row.EstoqueMinimo,
                row.SaldoAtual,
                row.ValorUnitario,
                origensPorChave.TryGetValue((row.UnidadeId, row.ProdutoId), out var origens)
                    ? origens
                    : Array.Empty<string>()))
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

    public async Task<decimal> GetSaldoByLoteAsync(
        Guid empresaId,
        Guid loteProdutoId,
        CancellationToken cancellationToken = default)
    {
        var movimentacoes = _context.MovimentacoesEstoque
            .AsNoTracking()
            .Where(movimentacao =>
                movimentacao.EmpresaId == empresaId
                && movimentacao.LoteProdutoId == loteProdutoId);

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

    public async Task<IReadOnlyList<LotBalanceRow>> ListLotBalancesAsync(
        Guid empresaId,
        Guid? unidadeId,
        Guid? produtoId,
        CancellationToken cancellationToken = default)
    {
        var balancesQuery = _context.MovimentacoesEstoque
            .AsNoTracking()
            .Where(movimentacao =>
                movimentacao.EmpresaId == empresaId
                && movimentacao.LoteProdutoId != null);

        if (unidadeId.HasValue)
        {
            balancesQuery = balancesQuery.Where(movimentacao => movimentacao.UnidadeId == unidadeId.Value);
        }

        if (produtoId.HasValue)
        {
            balancesQuery = balancesQuery.Where(movimentacao => movimentacao.ProdutoId == produtoId.Value);
        }

        var grouped = balancesQuery
            .GroupBy(movimentacao => movimentacao.LoteProdutoId!.Value)
            .Select(group => new
            {
                LoteProdutoId = group.Key,
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
            join lote in _context.LotesProduto.AsNoTracking() on balance.LoteProdutoId equals lote.Id
            join produto in _context.Produtos.AsNoTracking() on lote.ProdutoId equals produto.Id
            join unidade in _context.Unidades.AsNoTracking() on lote.UnidadeId equals unidade.Id
            join unidadeMedida in _context.UnidadesMedida.AsNoTracking() on produto.UnidadeMedidaId equals unidadeMedida.Id
            where lote.EmpresaId == empresaId && balance.SaldoAtual != 0
            orderby lote.DataValidade, lote.Codigo
            select new
            {
                lote.Id,
                lote.UnidadeId,
                UnidadeNome = unidade.Nome,
                lote.ProdutoId,
                ProdutoNome = produto.Nome,
                lote.Codigo,
                lote.DataValidade,
                balance.SaldoAtual,
                UnidadeMedidaSigla = unidadeMedida.Sigla,
                produto.ConteudoPorEmbalagem,
                produto.ConcentracaoPorConteudo
            };

        var rows = await query.ToListAsync(cancellationToken);

        return rows
            .Select(row => new LotBalanceRow(
                row.Id,
                row.UnidadeId,
                row.UnidadeNome,
                row.ProdutoId,
                row.ProdutoNome,
                row.Codigo,
                row.DataValidade,
                row.SaldoAtual,
                row.UnidadeMedidaSigla,
                row.ConteudoPorEmbalagem is > 0 && row.ConcentracaoPorConteudo is > 0
                    ? row.ConteudoPorEmbalagem * row.ConcentracaoPorConteudo
                    : null))
            .ToList();
    }

    public async Task<IReadOnlyList<(Guid LoteProdutoId, DateOnly DataValidade, DateTime CriadoEm, decimal Saldo)>> ListLotsWithBalanceFefoAsync(
        Guid empresaId,
        Guid unidadeId,
        Guid produtoId,
        CancellationToken cancellationToken = default)
    {
        var balances = await ListLotBalancesAsync(empresaId, unidadeId, produtoId, cancellationToken);

        return balances
            .Where(row => row.SaldoAtual > 0)
            .OrderBy(row => row.DataValidade)
            .ThenBy(row => row.Codigo)
            .Select(row => (row.LoteProdutoId, row.DataValidade, DateTime.MinValue, row.SaldoAtual))
            .ToList();
    }
}
