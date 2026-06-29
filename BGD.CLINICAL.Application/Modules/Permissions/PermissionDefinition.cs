namespace BGD.CLINICAL.Application.Modules.Permissions;

public sealed record PermissionDefinition(
    string Key,
    string Description,
    string Category,
    string Module,
    int Order = 0,
    string? Parent = null,
    string[]? Implies = null);
