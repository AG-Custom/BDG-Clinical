namespace BGD.CLINICAL.Application.Modules.Abstractions;

public interface IModuleLicensesRepository
{
    Task<bool> HasActiveLicenseAsync(
        Guid empresaId,
        string moduleCode,
        CancellationToken cancellationToken = default);
}
