using BGD.CLINICAL.Application.Modules.Abstractions;
using BGD.CLINICAL.Application.Modules.Permissions;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace BGD.CLINICAL.Infra.Data.Repositories.Modules;

public sealed class PermissionCatalogRepository : IPermissionCatalogRepository
{
    private readonly AppDbContext _context;

    public PermissionCatalogRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PermissaoSistema>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.PermissoesSistema
            .AsNoTracking()
            .OrderBy(permissao => permissao.Categoria)
            .ThenBy(permissao => permissao.Ordem)
            .ThenBy(permissao => permissao.Chave)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> SyncCatalogAsync(
        IReadOnlyList<PermissionDefinition> definitions,
        CancellationToken cancellationToken = default)
    {
        var existing = await _context.PermissoesSistema
            .ToListAsync(cancellationToken);

        var existingByKey = existing.ToDictionary(
            permissao => permissao.Chave,
            StringComparer.OrdinalIgnoreCase);

        if (IsCatalogInSync(definitions, existingByKey))
        {
            return false;
        }

        var hasChanges = false;

        foreach (var definition in definitions)
        {
            if (existingByKey.TryGetValue(definition.Key, out var permissao))
            {
                if (!NeedsUpdate(permissao, definition))
                {
                    continue;
                }

                permissao.UpdateMetadata(
                    definition.Description,
                    definition.Category,
                    definition.Module,
                    definition.Order,
                    definition.Parent);

                hasChanges = true;
                continue;
            }

            await _context.PermissoesSistema.AddAsync(
                new PermissaoSistema(
                    definition.Key,
                    definition.Description,
                    definition.Category,
                    definition.Module,
                    definition.Order,
                    definition.Parent),
                cancellationToken);

            hasChanges = true;
        }

        return hasChanges;
    }

    private static bool IsCatalogInSync(
        IReadOnlyList<PermissionDefinition> definitions,
        IReadOnlyDictionary<string, PermissaoSistema> existingByKey)
    {
        if (existingByKey.Count != definitions.Count)
        {
            return false;
        }

        foreach (var definition in definitions)
        {
            if (!existingByKey.TryGetValue(definition.Key, out var permissao)
                || NeedsUpdate(permissao, definition))
            {
                return false;
            }
        }

        return true;
    }

    private static bool NeedsUpdate(PermissaoSistema permissao, PermissionDefinition definition)
    {
        return !string.Equals(permissao.Descricao, definition.Description, StringComparison.Ordinal)
            || !string.Equals(permissao.Categoria, definition.Category, StringComparison.Ordinal)
            || !string.Equals(permissao.ModuloCodigo, definition.Module, StringComparison.OrdinalIgnoreCase)
            || permissao.Ordem != definition.Order
            || !string.Equals(permissao.ChavePai, definition.Parent, StringComparison.OrdinalIgnoreCase);
    }
}
