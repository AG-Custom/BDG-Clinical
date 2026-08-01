using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Inventory.Abstractions;
using AG.CLINICAL.Application.Inventory.Dtos;
using AG.CLINICAL.Domain.Enums;

namespace AG.CLINICAL.Application.Inventory.MeasurementUnits;

public interface IListMeasurementUnitsService
{
    Task<Result<IReadOnlyList<MeasurementUnitDto>>> ExecuteAsync(
        bool includeInactive,
        string? tipo,
        string? search,
        int? limit,
        CancellationToken cancellationToken = default);
}

public sealed class ListMeasurementUnitsService : IListMeasurementUnitsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IMeasurementUnitsRepository _measurementUnitsRepository;

    public ListMeasurementUnitsService(
        ICurrentTenantContext tenantContext,
        IMeasurementUnitsRepository measurementUnitsRepository)
    {
        _tenantContext = tenantContext;
        _measurementUnitsRepository = measurementUnitsRepository;
    }

    public async Task<Result<IReadOnlyList<MeasurementUnitDto>>> ExecuteAsync(
        bool includeInactive,
        string? tipo,
        string? search,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        TipoUnidadeMedida? tipoFiltro = null;

        if (!string.IsNullOrWhiteSpace(tipo))
        {
            var tipoResult = MeasurementUnitValidation.ParseTipo(tipo);
            if (tipoResult.IsFailure)
            {
                return Result<IReadOnlyList<MeasurementUnitDto>>.Failure(tipoResult.Error!);
            }

            tipoFiltro = tipoResult.Value;
        }

        string? normalizedSearch = null;
        int? effectiveLimit = null;

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchResult = MeasurementUnitValidation.ValidateSearch(search, limit);
            if (searchResult.IsFailure)
            {
                return Result<IReadOnlyList<MeasurementUnitDto>>.Failure(searchResult.Error!);
            }

            (normalizedSearch, var validatedLimit) = searchResult.Value!;
            effectiveLimit = validatedLimit;
        }
        else if (limit.HasValue)
        {
            var limitResult = MeasurementUnitValidation.ValidateLimit(limit, MeasurementUnitSearchOptions.DefaultLimit);
            if (limitResult.IsFailure)
            {
                return Result<IReadOnlyList<MeasurementUnitDto>>.Failure(limitResult.Error!);
            }

            effectiveLimit = limitResult.Value;
        }

        var unidades = await _measurementUnitsRepository.ListByEmpresaIdAsync(
            _tenantContext.EmpresaId,
            includeInactive,
            tipoFiltro,
            normalizedSearch,
            effectiveLimit,
            cancellationToken);

        return Result<IReadOnlyList<MeasurementUnitDto>>.Success(MeasurementUnitsMapper.Map(unidades));
    }
}
