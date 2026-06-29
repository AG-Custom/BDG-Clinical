using BGD.CLINICAL.Application.Modules.Abstractions;
using BGD.CLINICAL.Application.Modules.Permissions;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using Microsoft.Extensions.Caching.Memory;

namespace BGD.CLINICAL.Infra.Data.Services.Permissions;

public sealed class CachedPermissionChecker : IPermissionChecker, IPermissionCacheInvalidator
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(12);

    private readonly IMemoryCache _cache;
    private readonly IUserPermissionAssignmentsRepository _assignmentsRepository;
    private readonly IReadOnlyDictionary<string, string[]> _impliesByGrant;
    private readonly IReadOnlyDictionary<string, string> _parentByKey;
    private readonly IReadOnlyList<string> _catalogKeys;

    public CachedPermissionChecker(
        IMemoryCache cache,
        IUserPermissionAssignmentsRepository assignmentsRepository)
    {
        _cache = cache;
        _assignmentsRepository = assignmentsRepository;
        _impliesByGrant = PermissionResolver.BuildImpliesMap(PermissionCatalog.All);
        _parentByKey = PermissionResolver.BuildParentByKeyMap(PermissionCatalog.All);
        _catalogKeys = PermissionCatalog.All.Select(definition => definition.Key).ToList();
    }

    public async Task<bool> HasPermissionAsync(
        Guid usuarioId,
        string permission,
        CancellationToken cancellationToken = default)
    {
        var snapshot = await GetSnapshotAsync(usuarioId, cancellationToken);

        if (snapshot.IsAdmin)
        {
            return true;
        }

        return PermissionResolver.HasPermission(
            permission,
            snapshot.Grants,
            snapshot.Denials,
            _impliesByGrant,
            _parentByKey);
    }

    public async Task<IReadOnlySet<string>> GetEffectivePermissionsAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default)
    {
        var snapshot = await GetSnapshotAsync(usuarioId, cancellationToken);

        if (snapshot.IsAdmin)
        {
            return _catalogKeys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        return PermissionResolver.ResolveEffectiveKeys(
            snapshot.Grants,
            snapshot.Denials,
            _impliesByGrant,
            _parentByKey,
            _catalogKeys);
    }

    public void Invalidate(Guid usuarioId)
    {
        _cache.Remove(BuildCacheKey(usuarioId));
    }

    public void InvalidateUsuario(Guid usuarioId) => Invalidate(usuarioId);

    public void InvalidateUsuarios(IEnumerable<Guid> usuarioIds)
    {
        foreach (var usuarioId in usuarioIds)
        {
            Invalidate(usuarioId);
        }
    }

    private async Task<PermissionSnapshot> GetSnapshotAsync(Guid usuarioId, CancellationToken cancellationToken)
    {
        var cacheKey = BuildCacheKey(usuarioId);

        if (_cache.TryGetValue(cacheKey, out PermissionSnapshot? cached) && cached is not null)
        {
            return cached;
        }

        var assignment = await _assignmentsRepository.GetByUsuarioIdAsync(usuarioId, cancellationToken);

        if (assignment is null)
        {
            var denied = new PermissionSnapshot(false, [], []);
            _cache.Set(cacheKey, denied, CacheDuration);
            return denied;
        }

        if (assignment.TipoUsuario == TipoUsuario.Admin)
        {
            var adminSnapshot = new PermissionSnapshot(true, [], []);
            _cache.Set(cacheKey, adminSnapshot, CacheDuration);
            return adminSnapshot;
        }

        var grants = new List<string>(assignment.CargoKeys);
        var denials = new List<string>();

        foreach (var overrideItem in assignment.Overrides)
        {
            if (overrideItem.Effect == PermissionEffect.Allow)
            {
                grants.Add(overrideItem.PermissionKey);
                continue;
            }

            denials.Add(overrideItem.PermissionKey);
        }

        var snapshot = new PermissionSnapshot(false, grants, denials);
        _cache.Set(cacheKey, snapshot, CacheDuration);
        return snapshot;
    }

    private static string BuildCacheKey(Guid usuarioId) => $"permissions:{usuarioId}";

    private sealed record PermissionSnapshot(
        bool IsAdmin,
        IReadOnlyList<string> Grants,
        IReadOnlyList<string> Denials);
}
