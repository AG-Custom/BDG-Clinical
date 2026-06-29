using BGD.CLINICAL.Application.Modules;
using BGD.CLINICAL.Application.Modules.Abstractions;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace BGD.CLINICAL.Infra.Data.Repositories.Modules;

public sealed class SystemModulesRepository : ISystemModulesRepository
{
    private readonly AppDbContext _context;

    public SystemModulesRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> SyncCatalogAsync(CancellationToken cancellationToken = default)
    {
        var existing = await _context.ModulosSistema
            .ToListAsync(cancellationToken);

        var existingByCode = existing.ToDictionary(
            modulo => modulo.Codigo,
            StringComparer.OrdinalIgnoreCase);

        if (IsCatalogInSync(existingByCode))
        {
            return false;
        }

        var hasChanges = false;

        foreach (var definition in SystemModulesCatalog.All)
        {
            if (existingByCode.ContainsKey(definition.Code))
            {
                continue;
            }

            await _context.ModulosSistema.AddAsync(
                new ModuloSistema(definition.Name, definition.Code, definition.Description),
                cancellationToken);

            hasChanges = true;
        }

        return hasChanges;
    }

    public async Task<IReadOnlyList<ModuloSistema>> ListActiveAsync(CancellationToken cancellationToken = default)
    {
        var codes = SystemModulesCatalog.All
            .Select(definition => definition.Code)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return await _context.ModulosSistema
            .AsNoTracking()
            .Where(modulo => modulo.Ativo && codes.Contains(modulo.Codigo))
            .ToListAsync(cancellationToken);
    }

    private static bool IsCatalogInSync(IReadOnlyDictionary<string, ModuloSistema> existingByCode)
    {
        foreach (var definition in SystemModulesCatalog.All)
        {
            if (!existingByCode.ContainsKey(definition.Code))
            {
                return false;
            }
        }

        return true;
    }
}
