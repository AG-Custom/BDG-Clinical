namespace BGD.CLINICAL.Application.Modules.Dtos;

public sealed record PermissionMapNodeDto(
    string Key,
    string Description,
    string Category,
    string ModuleCode,
    int Order,
    string? Parent,
    IReadOnlyList<PermissionMapNodeDto> Children);

public sealed record PositionPermissionsDto(
    Guid Id,
    string Nome,
    IReadOnlyList<string> PermissionKeys);

public sealed record UpdatePositionPermissionsRequest(
    IReadOnlyList<string> PermissionKeys);

public sealed record EmployeePermissionsDto(
    Guid EmployeeId,
    Guid? UsuarioId,
    Guid? CargoId,
    string? CargoNome,
    IReadOnlyList<string> CargoPermissionKeys,
    IReadOnlyList<string> Allows,
    IReadOnlyList<string> Denies,
    IReadOnlyList<string> EffectivePermissions);

public sealed record UpdateEmployeePermissionsRequest(
    IReadOnlyList<string> Allows,
    IReadOnlyList<string> Denies);
