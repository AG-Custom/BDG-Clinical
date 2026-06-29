namespace BGD.CLINICAL.Application.Modules.Abstractions;

public interface IPermissionChecker
{
    Task<bool> HasPermissionAsync(Guid usuarioId, string permission, CancellationToken cancellationToken = default);

    Task<IReadOnlySet<string>> GetEffectivePermissionsAsync(Guid usuarioId, CancellationToken cancellationToken = default);

    void Invalidate(Guid usuarioId);
}
