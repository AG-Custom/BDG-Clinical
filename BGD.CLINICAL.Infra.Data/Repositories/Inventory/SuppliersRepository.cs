using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using BGD.CLINICAL.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace BGD.CLINICAL.Infra.Data.Repositories.Inventory;

public sealed class SuppliersRepository : ISuppliersRepository
{
    private readonly AppDbContext _context;

    public SuppliersRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Fornecedor>> ListByEmpresaIdAsync(
        Guid empresaId,
        bool includeInactive,
        string? search,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Fornecedores
            .AsNoTracking()
            .Where(fornecedor => fornecedor.EmpresaId == empresaId);

        if (!includeInactive)
        {
            query = query.Where(fornecedor => fornecedor.Ativo);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(fornecedor =>
                EF.Functions.Like(fornecedor.Nome, pattern) ||
                EF.Functions.Like(fornecedor.Cnpj, pattern));
        }

        var orderedQuery = query.OrderBy(fornecedor => fornecedor.Nome);

        if (limit.HasValue)
        {
            return await orderedQuery
                .Take(limit.Value)
                .ToListAsync(cancellationToken);
        }

        return await orderedQuery.ToListAsync(cancellationToken);
    }

    public Task<Fornecedor?> GetByIdAndEmpresaIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return _context.Fornecedores
            .FirstOrDefaultAsync(
                fornecedor => fornecedor.Id == id && fornecedor.EmpresaId == empresaId,
                cancellationToken);
    }

    public Task<bool> ExistsByNomeAsync(
        Guid empresaId,
        string nome,
        Guid? excludeId,
        CancellationToken cancellationToken = default)
    {
        var normalizedNome = nome.Trim().ToUpperInvariant();

        return _context.Fornecedores.AnyAsync(
            fornecedor => fornecedor.EmpresaId == empresaId
                && fornecedor.Nome.ToUpper() == normalizedNome
                && (!excludeId.HasValue || fornecedor.Id != excludeId.Value),
            cancellationToken);
    }

    public Task<bool> ExistsByCnpjAsync(
        Guid empresaId,
        string cnpj,
        Guid? excludeId,
        CancellationToken cancellationToken = default)
    {
        return _context.Fornecedores.AnyAsync(
            fornecedor => fornecedor.EmpresaId == empresaId
                && fornecedor.Cnpj == cnpj
                && (!excludeId.HasValue || fornecedor.Id != excludeId.Value),
            cancellationToken);
    }

    public Task<bool> ExistsActiveByIdAndEmpresaIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return _context.Fornecedores.AnyAsync(
            fornecedor => fornecedor.Id == id && fornecedor.EmpresaId == empresaId && fornecedor.Ativo,
            cancellationToken);
    }

    public Task<bool> HasOpenPedidosAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return _context.PedidosFornecedor.AnyAsync(
            pedido => pedido.FornecedorId == id
                && pedido.EmpresaId == empresaId
                && (pedido.Status == StatusPedidoFornecedor.Pendente
                    || pedido.Status == StatusPedidoFornecedor.EnviadoParaFornecedor),
            cancellationToken);
    }

    public async Task AddAsync(Fornecedor fornecedor, CancellationToken cancellationToken = default)
    {
        await _context.Fornecedores.AddAsync(fornecedor, cancellationToken);
    }

    public void Update(Fornecedor fornecedor)
    {
        var entry = _context.Entry(fornecedor);

        if (entry.State == EntityState.Detached)
        {
            _context.Fornecedores.Update(fornecedor);
        }
    }
}
