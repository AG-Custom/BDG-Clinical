namespace AG.CLINICAL.Application.Inventory.Abstractions;

public interface ICompanyDefaultProductTypesProvisioner
{
    Task ProvisionDefaultProductTypesAsync(Guid empresaId, CancellationToken cancellationToken = default);

    Task BackfillAllCompaniesAsync(CancellationToken cancellationToken = default);
}
