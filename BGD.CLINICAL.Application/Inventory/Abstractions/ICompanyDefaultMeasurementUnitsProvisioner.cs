namespace BGD.CLINICAL.Application.Inventory.Abstractions;

public interface ICompanyDefaultMeasurementUnitsProvisioner
{
    Task ProvisionDefaultMeasurementUnitsAsync(Guid empresaId, CancellationToken cancellationToken = default);

    Task BackfillAllCompaniesAsync(CancellationToken cancellationToken = default);
}
