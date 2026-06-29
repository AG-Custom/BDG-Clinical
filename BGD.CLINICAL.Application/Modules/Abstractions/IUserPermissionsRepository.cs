namespace BGD.CLINICAL.Application.Modules.Abstractions;

public interface IUserPermissionsRepository
{
    Task ReplaceOverridesAsync(
        Guid usuarioId,
        IReadOnlyList<string> allows,
        IReadOnlyList<string> denies,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<string> Allows, IReadOnlyList<string> Denies)> GetOverridesAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default);
}
