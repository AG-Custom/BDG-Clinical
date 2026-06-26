using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using BGD.CLINICAL.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace BGD.CLINICAL.Infra.Data.Repositories.Inventory;

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
            .FirstOrDefaultAsync(
                movimentacao => movimentacao.Id == id && movimentacao.EmpresaId == empresaId,
                cancellationToken);
    }
}
