using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Application.Inventory.Dtos;

namespace BGD.CLINICAL.Application.Inventory.MeasurementUnits;

public interface IGetMeasurementUnitsService
{
    Task<Result<MeasurementUnitDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

public sealed class GetMeasurementUnitsService : IGetMeasurementUnitsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IMeasurementUnitsRepository _measurementUnitsRepository;

    public GetMeasurementUnitsService(
        ICurrentTenantContext tenantContext,
        IMeasurementUnitsRepository measurementUnitsRepository)
    {
        _tenantContext = tenantContext;
        _measurementUnitsRepository = measurementUnitsRepository;
    }

    public async Task<Result<MeasurementUnitDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var unidadeMedida = await _measurementUnitsRepository.GetByIdAndEmpresaIdAsync(
            id,
            _tenantContext.EmpresaId,
            cancellationToken);

        if (unidadeMedida is null)
        {
            return Result<MeasurementUnitDto>.Failure("Unidade de medida não encontrada.");
        }

        return Result<MeasurementUnitDto>.Success(MeasurementUnitsMapper.Map(unidadeMedida));
    }
}
