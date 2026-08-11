using AG.CLINICAL.Application.Inventory.Abstractions;
using AG.CLINICAL.Domain.Entities;
using AG.CLINICAL.Domain.Exceptions;
using AG.CLINICAL.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace AG.CLINICAL.Infra.Data.Repositories.Inventory;

public sealed class ProductLotsRepository : IProductLotsRepository
{
    private readonly AppDbContext _context;

    public ProductLotsRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<LoteProduto?> GetByCodigoAsync(
        Guid empresaId,
        Guid unidadeId,
        Guid produtoId,
        string codigo,
        CancellationToken cancellationToken = default)
    {
        var normalized = codigo.Trim().ToUpperInvariant();

        return _context.LotesProduto.FirstOrDefaultAsync(
            lote => lote.EmpresaId == empresaId
                && lote.UnidadeId == unidadeId
                && lote.ProdutoId == produtoId
                && lote.Codigo.ToUpper() == normalized,
            cancellationToken);
    }

    public Task<LoteProduto?> GetByIdAndEmpresaIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return _context.LotesProduto.FirstOrDefaultAsync(
            lote => lote.Id == id && lote.EmpresaId == empresaId,
            cancellationToken);
    }

    public async Task AddAsync(LoteProduto lote, CancellationToken cancellationToken = default)
    {
        await _context.LotesProduto.AddAsync(lote, cancellationToken);
    }

    public async Task<LoteProduto> GetOrCreateAsync(
        Guid empresaId,
        Guid unidadeId,
        Guid produtoId,
        string codigo,
        DateOnly dataValidade,
        CancellationToken cancellationToken = default)
    {
        var existente = await GetByCodigoAsync(empresaId, unidadeId, produtoId, codigo, cancellationToken);
        if (existente is not null)
        {
            return existente;
        }

        var lote = LoteProduto.Create(empresaId, unidadeId, produtoId, codigo, dataValidade);

        try
        {
            await _context.LotesProduto.AddAsync(lote, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return lote;
        }
        catch (DbUpdateException)
        {
            _context.Entry(lote).State = EntityState.Detached;

            var criadoEmParalelo = await GetByCodigoAsync(
                empresaId,
                unidadeId,
                produtoId,
                codigo,
                cancellationToken);

            if (criadoEmParalelo is null)
            {
                throw new DomainException(
                    $"Não foi possível criar o lote \"{codigo.Trim()}\" na unidade de destino para concluir a transferência. " +
                    "Tente novamente em instantes.");
            }

            return criadoEmParalelo;
        }
    }
}
