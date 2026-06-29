namespace BGD.CLINICAL.Application.Modules.Permissions;

public static class PermissionResolver
{
    public static bool HasPermission(
        string permission,
        IReadOnlyCollection<string> grants,
        IReadOnlyCollection<string> denials,
        IReadOnlyDictionary<string, string[]> impliesByGrant,
        IReadOnlyDictionary<string, string> parentByKey)
    {
        if (string.IsNullOrWhiteSpace(permission))
        {
            return false;
        }

        if (IsDenied(permission, denials))
        {
            return false;
        }

        return IsGranted(permission, grants, impliesByGrant, parentByKey);
    }

    public static IReadOnlySet<string> ResolveEffectiveKeys(
        IReadOnlyCollection<string> grants,
        IReadOnlyCollection<string> denials,
        IReadOnlyDictionary<string, string[]> impliesByGrant,
        IReadOnlyDictionary<string, string> parentByKey,
        IReadOnlyCollection<string> catalogKeys)
    {
        var effective = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var key in catalogKeys)
        {
            if (HasPermission(key, grants, denials, impliesByGrant, parentByKey))
            {
                effective.Add(key);
            }
        }

        return effective;
    }

    private static bool IsGranted(
        string permission,
        IReadOnlyCollection<string> grants,
        IReadOnlyDictionary<string, string[]> impliesByGrant,
        IReadOnlyDictionary<string, string> parentByKey)
    {
        foreach (var grant in grants)
        {
            if (MatchesPattern(grant, permission))
            {
                return true;
            }

            if (ImpliesPermission(grant, permission, impliesByGrant, parentByKey))
            {
                return true;
            }

            if (IsDescendantGrantOf(grant, permission, parentByKey))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsDescendantGrantOf(
        string grant,
        string ancestorPermission,
        IReadOnlyDictionary<string, string> parentByKey)
    {
        var current = grant;
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        while (parentByKey.TryGetValue(current, out var parent) && !string.IsNullOrWhiteSpace(parent))
        {
            if (!visited.Add(current))
            {
                break;
            }

            if (string.Equals(parent, ancestorPermission, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            current = parent;
        }

        return false;
    }

    private static bool IsDenied(string permission, IReadOnlyCollection<string> denials)
    {
        foreach (var denial in denials)
        {
            if (MatchesPattern(denial, permission))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ImpliesPermission(
        string grant,
        string permission,
        IReadOnlyDictionary<string, string[]> impliesByGrant,
        IReadOnlyDictionary<string, string> parentByKey)
    {
        if (!impliesByGrant.TryGetValue(grant, out var implied))
        {
            return false;
        }

        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var stack = new Stack<string>(implied);

        while (stack.Count > 0)
        {
            var current = stack.Pop();

            if (!visited.Add(current))
            {
                continue;
            }

            if (string.Equals(current, permission, StringComparison.OrdinalIgnoreCase)
                || IsDescendantGrantOf(current, permission, parentByKey))
            {
                return true;
            }

            if (impliesByGrant.TryGetValue(current, out var nested))
            {
                foreach (var next in nested)
                {
                    stack.Push(next);
                }
            }
        }

        return false;
    }

    public static bool MatchesPattern(string pattern, string permission)
    {
        if (string.Equals(pattern, permission, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.Equals(pattern, "*", StringComparison.Ordinal))
        {
            return true;
        }

        if (!pattern.EndsWith(".*", StringComparison.Ordinal))
        {
            return false;
        }

        var prefix = pattern[..^2];

        return permission.StartsWith(prefix + ".", StringComparison.OrdinalIgnoreCase)
            || string.Equals(permission, prefix, StringComparison.OrdinalIgnoreCase);
    }

    public static IReadOnlyDictionary<string, string[]> BuildImpliesMap(
        IReadOnlyList<PermissionDefinition> definitions)
    {
        return definitions
            .Where(definition => definition.Implies is { Length: > 0 })
            .ToDictionary(
                definition => definition.Key,
                definition => definition.Implies!,
                StringComparer.OrdinalIgnoreCase);
    }

    public static IReadOnlyDictionary<string, string> BuildParentByKeyMap(
        IReadOnlyList<PermissionDefinition> definitions)
    {
        return definitions
            .Where(definition => !string.IsNullOrWhiteSpace(definition.Parent))
            .ToDictionary(
                definition => definition.Key,
                definition => definition.Parent!,
                StringComparer.OrdinalIgnoreCase);
    }

    public static IReadOnlyDictionary<string, string> BuildModuleByKeyMap(
        IReadOnlyList<PermissionDefinition> definitions)
    {
        return definitions.ToDictionary(
            definition => definition.Key,
            definition => definition.Module,
            StringComparer.OrdinalIgnoreCase);
    }
}
