using AG.CLINICAL.Domain.Entities;
using AG.CLINICAL.Domain.Enums;

namespace AG.CLINICAL.Application.Modules.Abstractions;

public sealed record UserPermissionAssignment(
    Guid UsuarioId,
    TipoUsuario TipoUsuario,
    IReadOnlyList<string> CargoKeys,
    IReadOnlyList<UserPermissionOverride> Overrides);

public sealed record UserPermissionOverride(string PermissionKey, PermissionEffect Effect);

public interface IUserPermissionAssignmentsRepository
{
    Task<UserPermissionAssignment?> GetByUsuarioIdAsync(Guid usuarioId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Guid>> ListUsuarioIdsByCargoIdAsync(
        Guid empresaId,
        Guid cargoId,
        CancellationToken cancellationToken = default);
}
