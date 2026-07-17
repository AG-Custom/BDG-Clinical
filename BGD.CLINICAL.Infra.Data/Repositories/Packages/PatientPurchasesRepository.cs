using BGD.CLINICAL.Application.Packages.Abstractions;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using BGD.CLINICAL.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace BGD.CLINICAL.Infra.Data.Repositories.Packages;

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
        var query = _context.ComprasPaciente
            .AsNoTracking()
            .Include(compra => compra.Pacote)
                .ThenInclude(pacote => pacote.Itens)
                    .ThenInclude(item => item.Produto)
            .Include(compra => compra.Unidade)
            .Include(compra => compra.Paciente)
            .Include(compra => compra.Aplicacoes)
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
        return _context.ComprasPaciente
            .Include(compra => compra.Pacote)
                .ThenInclude(pacote => pacote.Itens)
                    .ThenInclude(item => item.Produto)
            .Include(compra => compra.Unidade)
            .Include(compra => compra.Paciente)
            .Include(compra => compra.Aplicacoes)
            .FirstOrDefaultAsync(
                compra => compra.Id == id && compra.EmpresaId == empresaId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<CompraPaciente>> ListActiveByPacienteAsync(
        Guid empresaId,
        Guid pacienteId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ComprasPaciente
            .AsNoTracking()
            .Include(compra => compra.Pacote)
                .ThenInclude(pacote => pacote.Itens)
                    .ThenInclude(item => item.Produto)
            .Include(compra => compra.Unidade)
            .Include(compra => compra.Aplicacoes)
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
        _context.ComprasPaciente.Update(compra);
    }
}
