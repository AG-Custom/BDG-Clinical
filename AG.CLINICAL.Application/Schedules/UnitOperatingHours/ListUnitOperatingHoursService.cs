using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Core.Abstractions;
using AG.CLINICAL.Application.Schedules.Abstractions;
using AG.CLINICAL.Application.Schedules.Dtos;
using AG.CLINICAL.Domain.Exceptions;

namespace AG.CLINICAL.Application.Schedules.UnitOperatingHours;

public interface IListUnitOperatingHoursService
{
    Task<Result<IReadOnlyList<UnitOperatingHourDto>>> ExecuteAsync(
        Guid unitId,
        bool includeInactive = false,
        CancellationToken cancellationToken = default);
}

public sealed class ListUnitOperatingHoursService : IListUnitOperatingHoursService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IUnitsRepository _unitsRepository;
    private readonly IUnitOperatingHoursRepository _operatingHoursRepository;

    public ListUnitOperatingHoursService(
        ICurrentTenantContext tenantContext,
        IUnitsRepository unitsRepository,
        IUnitOperatingHoursRepository operatingHoursRepository)
    {
        _tenantContext = tenantContext;
        _unitsRepository = unitsRepository;
        _operatingHoursRepository = operatingHoursRepository;
    }

    public async Task<Result<IReadOnlyList<UnitOperatingHourDto>>> ExecuteAsync(
        Guid unitId,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;

        var unitValidation = await UnitOperatingHoursValidator.EnsureUnitExistsAsync(
            empresaId,
            unitId,
            _unitsRepository,
            cancellationToken);

        if (unitValidation.IsFailure)
        {
            return Result<IReadOnlyList<UnitOperatingHourDto>>.Failure(unitValidation.Error!);
        }

        var horarios = await _operatingHoursRepository.ListByUnitIdAsync(
            empresaId,
            unitId,
            includeInactive,
            cancellationToken);

        var dtos = horarios.Select(UnitOperatingHoursMapper.Map).ToList();
        return Result<IReadOnlyList<UnitOperatingHourDto>>.Success(dtos);
    }
}
