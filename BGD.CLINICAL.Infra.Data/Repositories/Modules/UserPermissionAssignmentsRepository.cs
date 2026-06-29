using BGD.CLINICAL.Application.Modules.Abstractions;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using BGD.CLINICAL.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace BGD.CLINICAL.Infra.Data.Repositories.Modules;

public sealed class UserPermissionAssignmentsRepository : IUserPermissionAssignmentsRepository
{
    private readonly AppDbContext _context;

    public UserPermissionAssignmentsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserPermissionAssignment?> GetByUsuarioIdAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default)
    {
        var usuario = await _context.Usuarios
            .AsNoTracking()
            .Include(entity => entity.Funcionario!)
                .ThenInclude(funcionario => funcionario.Vinculos.Where(vinculo => vinculo.Ativo))
                    .ThenInclude(vinculo => vinculo.Cargo!)
                        .ThenInclude(cargo => cargo.Permissoes)
            .Include(entity => entity.Funcionario!)
                .ThenInclude(funcionario => funcionario.Vinculos.Where(vinculo => vinculo.Ativo))
                    .ThenInclude(vinculo => vinculo.Unidade)
            .Include(entity => entity.PermissoesOverride)
            .FirstOrDefaultAsync(entity => entity.Id == usuarioId, cancellationToken);

        if (usuario is null)
        {
            return null;
        }

        var cargoKeys = ResolveCargoPermissionKeys(usuario);

        var overrides = usuario.PermissoesOverride
            .Select(overrideItem => new UserPermissionOverride(overrideItem.PermissionKey, overrideItem.Effect))
            .ToList();

        return new UserPermissionAssignment(
            usuario.Id,
            usuario.TipoUsuario,
            cargoKeys,
            overrides);
    }

    public async Task<IReadOnlyList<Guid>> ListUsuarioIdsByCargoIdAsync(
        Guid empresaId,
        Guid cargoId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Usuarios
            .AsNoTracking()
            .Where(usuario =>
                usuario.EmpresaId == empresaId
                && usuario.FuncionarioId.HasValue
                && usuario.Funcionario!.Vinculos.Any(vinculo =>
                    vinculo.Ativo
                    && vinculo.CargoId == cargoId
                    && (vinculo.EmpresaId == empresaId
                        || (vinculo.UnidadeId.HasValue && vinculo.Unidade!.EmpresaId == empresaId))))
            .Select(usuario => usuario.Id)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    private static IReadOnlyList<string> ResolveCargoPermissionKeys(Usuario usuario)
    {
        if (usuario.Funcionario is null)
        {
            return [];
        }

        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var vinculo in usuario.Funcionario.Vinculos.Where(vinculo => vinculo.Ativo))
        {
            if (!vinculo.BelongsToEmpresa(usuario.EmpresaId))
            {
                continue;
            }

            if (vinculo.Cargo is not { Ativo: true })
            {
                continue;
            }

            foreach (var item in vinculo.Cargo.Permissoes)
            {
                keys.Add(item.PermissionKey);
            }
        }

        return keys.ToList();
    }
}

public sealed class UserPermissionsRepository : IUserPermissionsRepository
{
    private readonly AppDbContext _context;

    public UserPermissionsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task ReplaceOverridesAsync(
        Guid usuarioId,
        IReadOnlyList<string> allows,
        IReadOnlyList<string> denies,
        CancellationToken cancellationToken = default)
    {
        var existing = await _context.UsuarioPermissoesOverride
            .Where(entity => entity.UsuarioId == usuarioId)
            .ToListAsync(cancellationToken);

        _context.UsuarioPermissoesOverride.RemoveRange(existing);

        foreach (var key in allows.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                continue;
            }

            await _context.UsuarioPermissoesOverride.AddAsync(
                new UsuarioPermissaoOverride(usuarioId, key.Trim(), PermissionEffect.Allow),
                cancellationToken);
        }

        foreach (var key in denies.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                continue;
            }

            await _context.UsuarioPermissoesOverride.AddAsync(
                new UsuarioPermissaoOverride(usuarioId, key.Trim(), PermissionEffect.Deny),
                cancellationToken);
        }
    }

    public async Task<(IReadOnlyList<string> Allows, IReadOnlyList<string> Denies)> GetOverridesAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default)
    {
        var overrides = await _context.UsuarioPermissoesOverride
            .AsNoTracking()
            .Where(entity => entity.UsuarioId == usuarioId)
            .ToListAsync(cancellationToken);

        var allows = overrides
            .Where(entity => entity.Effect == PermissionEffect.Allow)
            .Select(entity => entity.PermissionKey)
            .ToList();

        var denies = overrides
            .Where(entity => entity.Effect == PermissionEffect.Deny)
            .Select(entity => entity.PermissionKey)
            .ToList();

        return (allows, denies);
    }
}
