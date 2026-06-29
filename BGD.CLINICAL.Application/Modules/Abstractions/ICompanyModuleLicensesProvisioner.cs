namespace BGD.CLINICAL.Application.Modules.Abstractions;

public interface ICompanyModuleLicensesProvisioner
{
    Task ProvisionAllModulesAsync(Guid empresaId, CancellationToken cancellationToken = default);

    Task BackfillAllCompaniesAsync(CancellationToken cancellationToken = default);
}
