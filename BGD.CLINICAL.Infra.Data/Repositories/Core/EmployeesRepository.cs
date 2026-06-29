using BGD.CLINICAL.Application.Abstractions.Identity;
using BGD.CLINICAL.Application.Core.Abstractions;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using BGD.CLINICAL.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace BGD.CLINICAL.Infra.Data.Repositories.Core;

public sealed class EmployeesRepository : IEmployeesRepository
{
    private readonly AppDbContext _context;

    public EmployeesRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Funcionario>> ListByEmpresaIdAsync(
        Guid empresaId,
        Guid? unidadeId,
        bool includeInactive,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Funcionarios
            .AsNoTracking()
            .Include(funcionario => funcionario.Vinculos)
                .ThenInclude(vinculo => vinculo.Unidade)
            .Include(funcionario => funcionario.Vinculos)
                .ThenInclude(vinculo => vinculo.Cargo)
            .Where(funcionario => funcionario.Vinculos.Any(vinculo =>
                vinculo.EmpresaId == empresaId
                || (vinculo.UnidadeId != null
                    && _context.Unidades.Any(unidade =>
                        unidade.Id == vinculo.UnidadeId
                        && unidade.EmpresaId == empresaId))));

        if (unidadeId.HasValue)
        {
            query = query.Where(funcionario => funcionario.Vinculos.Any(vinculo =>
                vinculo.Ativo
                && (vinculo.EmpresaId == empresaId
                    || (vinculo.UnidadeId == unidadeId.Value
                        && _context.Unidades.Any(unidade =>
                            unidade.Id == unidadeId.Value
                            && unidade.EmpresaId == empresaId)))));
        }

        if (!includeInactive)
        {
            query = query.Where(funcionario => funcionario.Ativo);
        }

        return await query
            .OrderBy(funcionario => funcionario.Nome)
            .ToListAsync(cancellationToken);
    }

    public Task<Funcionario?> GetByIdAndEmpresaIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return _context.Funcionarios
            .Include(funcionario => funcionario.Vinculos)
                .ThenInclude(vinculo => vinculo.Unidade)
            .Include(funcionario => funcionario.Vinculos)
                .ThenInclude(vinculo => vinculo.Cargo)
            .FirstOrDefaultAsync(
                funcionario => funcionario.Id == id
                    && funcionario.Vinculos.Any(vinculo =>
                        vinculo.EmpresaId == empresaId
                        || (vinculo.UnidadeId != null
                            && _context.Unidades.Any(unidade =>
                                unidade.Id == vinculo.UnidadeId
                                && unidade.EmpresaId == empresaId))),
                cancellationToken);
    }

    public async Task<bool> AllUnidadesBelongToEmpresaAsync(
        IReadOnlyList<Guid> unidadeIds,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        if (unidadeIds.Count == 0)
        {
            return false;
        }

        var distinctIds = unidadeIds.Distinct().ToList();
        var count = await _context.Unidades.CountAsync(
            unidade => distinctIds.Contains(unidade.Id)
                && unidade.EmpresaId == empresaId
                && unidade.Ativo,
            cancellationToken);

        return count == distinctIds.Count;
    }

    public async Task<EmployeeUserAccessInfo?> GetUserAccessInfoByFuncionarioAndEmpresaAsync(
        Guid funcionarioId,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return await GetUserAccessInfoAsync(funcionarioId, empresaId, cancellationToken);
    }

    public async Task<Guid?> GetUsuarioIdByFuncionarioAndEmpresaAsync(
        Guid funcionarioId,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        var usuario = await _context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(
                user => user.FuncionarioId == funcionarioId && user.EmpresaId == empresaId,
                cancellationToken);

        return usuario?.Id;
    }

    private async Task<EmployeeUserAccessInfo?> GetUserAccessInfoAsync(
        Guid funcionarioId,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        var usuario = await _context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(
                user => user.FuncionarioId == funcionarioId
                    && user.EmpresaId == empresaId,
                cancellationToken);

        return usuario is null
            ? null
            : new EmployeeUserAccessInfo(
                usuario.EmailLogin,
                usuario.PendentePrimeiroAcesso,
                usuario.TipoUsuario == TipoUsuario.Admin);
    }

    public async Task AddAsync(Funcionario funcionario, CancellationToken cancellationToken = default)
    {
        await _context.Funcionarios.AddAsync(funcionario, cancellationToken);
    }

    public void Update(Funcionario funcionario)
    {
        var entry = _context.Entry(funcionario);

        if (entry.State == EntityState.Detached)
        {
            _context.Funcionarios.Attach(funcionario);
            entry.State = EntityState.Modified;
        }

        foreach (var vinculo in funcionario.Vinculos)
        {
            EnsureVinculoTrackedCorrectly(vinculo);
        }
    }

    private void EnsureVinculoTrackedCorrectly(FuncionarioVinculo vinculo)
    {
        var vinculoEntry = _context.Entry(vinculo);

        if (vinculoEntry.State is EntityState.Added or EntityState.Deleted)
        {
            return;
        }

        if (_context.FuncionarioVinculos.Local.Any(tracked => tracked.Id == vinculo.Id))
        {
            if (vinculoEntry.State == EntityState.Modified && !VinculoExistsInDatabase(vinculo.Id))
            {
                _context.FuncionarioVinculos.Add(vinculo);
            }

            return;
        }

        if (VinculoExistsInDatabase(vinculo.Id))
        {
            if (vinculoEntry.State == EntityState.Detached)
            {
                _context.FuncionarioVinculos.Attach(vinculo);
            }

            vinculoEntry.State = EntityState.Modified;
            return;
        }

        _context.FuncionarioVinculos.Add(vinculo);
    }

    private bool VinculoExistsInDatabase(Guid vinculoId)
    {
        return _context.FuncionarioVinculos
            .AsNoTracking()
            .Any(vinculo => vinculo.Id == vinculoId);
    }
}
