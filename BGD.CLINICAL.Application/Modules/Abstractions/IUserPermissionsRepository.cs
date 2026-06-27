using BGD.CLINICAL.Domain.Enums;

namespace BGD.CLINICAL.Application.Modules.Abstractions;

public interface IUserPermissionsRepository
{
    Task<bool> HasPermissionAsync(
        Guid usuarioId,
        string moduleCode,
        ModulePermissionAction action,
        CancellationToken cancellationToken = default);
}
