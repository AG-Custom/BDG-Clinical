using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;

namespace BGD.CLINICAL.Application.Modules.Abstractions;

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
