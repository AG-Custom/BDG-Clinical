using BGD.CLINICAL.Application.Packages.Abstractions;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace BGD.CLINICAL.Infra.Data.Repositories.Packages;

public sealed class PackagesRepository : IPackagesRepository
{
    private readonly AppDbContext _context;

    public PackagesRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Pacote>> ListByEmpresaIdAsync(
        Guid empresaId,
        bool includeInactive,
        string? search,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Pacotes
            .AsNoTracking()
            .Include(pacote => pacote.Itens)
                .ThenInclude(item => item.Produto)
            .Where(pacote => pacote.EmpresaId == empresaId);

        if (!includeInactive)
        {
            query = query.Where(pacote => pacote.Ativo);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(pacote => EF.Functions.Like(pacote.Nome, pattern));
        }

        query = query.OrderBy(pacote => pacote.Nome);

        if (limit.HasValue)
        {
            query = query.Take(limit.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public Task<Pacote?> GetByIdAndEmpresaIdWithDetailsAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return _context.Pacotes
            .Include(pacote => pacote.Itens)
                .ThenInclude(item => item.Produto)
            .FirstOrDefaultAsync(
                pacote => pacote.Id == id && pacote.EmpresaId == empresaId,
                cancellationToken);
    }

    public Task<bool> ExistsByNomeAsync(
        Guid empresaId,
        string nome,
        Guid? excludeId,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Pacotes.AsNoTracking()
            .Where(pacote => pacote.EmpresaId == empresaId && pacote.Nome == nome);

        if (excludeId.HasValue)
        {
            query = query.Where(pacote => pacote.Id != excludeId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    public Task<bool> ExistsActiveByIdAndEmpresaIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return _context.Pacotes.AsNoTracking()
            .AnyAsync(
                pacote => pacote.Id == id
                    && pacote.EmpresaId == empresaId
                    && pacote.Ativo,
                cancellationToken);
    }

    public async Task AddAsync(Pacote pacote, CancellationToken cancellationToken = default)
    {
        await _context.Pacotes.AddAsync(pacote, cancellationToken);
    }

    public void Update(Pacote pacote)
    {
        var entry = _context.Entry(pacote);

        if (entry.State == EntityState.Detached)
        {
            _context.Pacotes.Attach(pacote);
            entry.State = EntityState.Modified;
        }

        var currentItemIds = pacote.Itens.Select(item => item.Id).ToHashSet();
        var orphans = _context.ItensPacote.Local
            .Where(item => item.PacoteId == pacote.Id && !currentItemIds.Contains(item.Id))
            .ToList();

        foreach (var orphan in orphans)
        {
            if (_context.Entry(orphan).State != EntityState.Deleted)
            {
                _context.ItensPacote.Remove(orphan);
            }
        }

        foreach (var item in pacote.Itens)
        {
            EnsureItemTrackedCorrectly(item);
        }
    }

    private void EnsureItemTrackedCorrectly(ItemPacote item)
    {
        var itemEntry = _context.Entry(item);

        if (itemEntry.State is EntityState.Added or EntityState.Deleted)
        {
            return;
        }

        if (_context.ItensPacote.Local.Any(tracked => tracked.Id == item.Id))
        {
            if (itemEntry.State == EntityState.Modified && !ItemExistsInDatabase(item.Id))
            {
                _context.ItensPacote.Add(item);
            }

            return;
        }

        if (ItemExistsInDatabase(item.Id))
        {
            if (itemEntry.State == EntityState.Detached)
            {
                _context.ItensPacote.Attach(item);
            }

            itemEntry.State = EntityState.Modified;
            return;
        }

        _context.ItensPacote.Add(item);
    }

    private bool ItemExistsInDatabase(Guid itemId)
    {
        return _context.ItensPacote
            .AsNoTracking()
            .Any(item => item.Id == itemId);
    }
}
