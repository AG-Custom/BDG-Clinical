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
        bool includeInactive,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Funcionarios
            .AsNoTracking()
            .Include(funcionario => funcionario.Vinculos)
                .ThenInclude(vinculo => vinculo.Unidade)
            .Where(funcionario => funcionario.Vinculos.Any(vinculo =>
                vinculo.EmpresaId == empresaId
                || (vinculo.UnidadeId != null
                    && _context.Unidades.Any(unidade =>
                        unidade.Id == vinculo.UnidadeId
                        && unidade.EmpresaId == empresaId))));

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
            _context.Funcionarios.Update(funcionario);
        }
    }
}
