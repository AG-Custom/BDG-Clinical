using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Application.Inventory.MeasurementUnits;
using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Infra.Data.Services.Inventory;

public sealed class CompanyDefaultMeasurementUnitsProvisioner : ICompanyDefaultMeasurementUnitsProvisioner
{
    private readonly IRepository<Empresa> _empresaRepository;
    private readonly IMeasurementUnitsRepository _measurementUnitsRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CompanyDefaultMeasurementUnitsProvisioner(
        IRepository<Empresa> empresaRepository,
        IMeasurementUnitsRepository measurementUnitsRepository,
        IUnitOfWork unitOfWork)
    {
        _empresaRepository = empresaRepository;
        _measurementUnitsRepository = measurementUnitsRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task ProvisionDefaultMeasurementUnitsAsync(
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        foreach (var template in DefaultMeasurementUnitsCatalog.All)
        {
            if (await _measurementUnitsRepository.ExistsBySiglaAsync(
                    empresaId,
                    template.Sigla,
                    excludeId: null,
                    cancellationToken))
            {
                continue;
            }

            var unidadeMedida = UnidadeMedida.Create(
                empresaId,
                template.Nome,
                template.Sigla,
                template.Tipo);

            await _measurementUnitsRepository.AddAsync(unidadeMedida, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task BackfillAllCompaniesAsync(CancellationToken cancellationToken = default)
    {
        var empresas = await _empresaRepository.ListAsync(cancellationToken);

        foreach (var empresa in empresas)
        {
            await ProvisionDefaultMeasurementUnitsAsync(empresa.Id, cancellationToken);
        }
    }
}
