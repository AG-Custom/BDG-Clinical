using AG.CLINICAL.Application.Packages.Abstractions;
using AG.CLINICAL.Domain.Entities;
using AG.CLINICAL.Domain.Enums;
using AG.CLINICAL.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace AG.CLINICAL.Infra.Data.Repositories.Packages;

public sealed class PatientPurchasesRepository : IPatientPurchasesRepository
{
    private readonly AppDbContext _context;

    public PatientPurchasesRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CompraPaciente>> ListByEmpresaIdAsync(
        Guid empresaId,
        Guid? pacienteId,
        StatusCompraPaciente? status,
        CancellationToken cancellationToken = default)
    {
        var query = QueryWithDetails()
            .AsNoTracking()
            .Where(compra => compra.EmpresaId == empresaId);

        if (pacienteId.HasValue && pacienteId.Value != Guid.Empty)
        {
            query = query.Where(compra => compra.PacienteId == pacienteId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(compra => compra.Status == status.Value);
        }

        return await query
            .OrderByDescending(compra => compra.DataCompra)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CompraPaciente>> ListTrackedByEmpresaIdAsync(
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return await QueryWithDetails()
            .Where(compra => compra.EmpresaId == empresaId)
            .OrderByDescending(compra => compra.DataCompra)
            .ToListAsync(cancellationToken);
    }

    public Task<IReadOnlyList<CompraPaciente>> ListByPacienteAsync(
        Guid empresaId,
        Guid pacienteId,
        StatusCompraPaciente? status,
        CancellationToken cancellationToken = default)
    {
        return ListByEmpresaIdAsync(empresaId, pacienteId, status, cancellationToken);
    }

    public Task<CompraPaciente?> GetByIdAndEmpresaIdWithDetailsAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return QueryWithDetails()
            .FirstOrDefaultAsync(
                compra => compra.Id == id && compra.EmpresaId == empresaId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<CompraPaciente>> ListActiveByPacienteAsync(
        Guid empresaId,
        Guid pacienteId,
        CancellationToken cancellationToken = default)
    {
        return await QueryWithDetails()
            .AsNoTracking()
            .Where(compra =>
                compra.EmpresaId == empresaId
                && compra.PacienteId == pacienteId
                && compra.Status == StatusCompraPaciente.Ativo)
            .OrderByDescending(compra => compra.DataCompra)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(CompraPaciente compra, CancellationToken cancellationToken = default)
    {
        await _context.ComprasPaciente.AddAsync(compra, cancellationToken);
    }

    public void Update(CompraPaciente compra)
    {
        var entry = _context.Entry(compra);

        if (entry.State == EntityState.Detached)
        {
            _context.ComprasPaciente.Attach(compra);
            entry.State = EntityState.Modified;
        }

        var currentItemIds = compra.Itens.Select(item => item.Id).ToHashSet();
        var orphans = _context.ItensCompraPaciente.Local
            .Where(item => item.CompraPacienteId == compra.Id && !currentItemIds.Contains(item.Id))
            .ToList();

        foreach (var orphan in orphans)
        {
            if (_context.Entry(orphan).State != EntityState.Deleted)
            {
                _context.ItensCompraPaciente.Remove(orphan);
            }
        }

        foreach (var item in compra.Itens)
        {
            EnsureItemTrackedCorrectly(item);
        }
    }

    private void EnsureItemTrackedCorrectly(ItemCompraPaciente item)
    {
        var itemEntry = _context.Entry(item);

        if (itemEntry.State is EntityState.Added or EntityState.Deleted)
        {
            return;
        }

        if (_context.ItensCompraPaciente.Local.Any(tracked => tracked.Id == item.Id))
        {
            if (itemEntry.State == EntityState.Modified && !ItemExistsInDatabase(item.Id))
            {
                _context.ItensCompraPaciente.Add(item);
            }

            return;
        }

        if (ItemExistsInDatabase(item.Id))
        {
            if (itemEntry.State == EntityState.Detached)
            {
                _context.ItensCompraPaciente.Attach(item);
            }

            itemEntry.State = EntityState.Modified;
            return;
        }

        _context.ItensCompraPaciente.Add(item);
    }

    private bool ItemExistsInDatabase(Guid itemId)
    {
        return _context.ItensCompraPaciente
            .AsNoTracking()
            .Any(item => item.Id == itemId);
    }

    public Task<int> CountByPacoteIdAsync(
        Guid empresaId,
        Guid pacoteId,
        CancellationToken cancellationToken = default)
    {
        return _context.ComprasPaciente
            .AsNoTracking()
            .CountAsync(
                compra => compra.EmpresaId == empresaId && compra.PacoteId == pacoteId,
                cancellationToken);
    }

    private IQueryable<CompraPaciente> QueryWithDetails()
    {
        return _context.ComprasPaciente
            .Include(compra => compra.Pacote)
                .ThenInclude(pacote => pacote.Itens)
                    .ThenInclude(item => item.Produto)
            .Include(compra => compra.Itens)
                .ThenInclude(item => item.Produto)
            .Include(compra => compra.Unidade)
            .Include(compra => compra.Paciente)
            .Include(compra => compra.Aplicacoes);
    }
}
