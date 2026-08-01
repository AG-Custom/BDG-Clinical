using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Core.Abstractions;
using AG.CLINICAL.Application.Core.Dtos;

namespace AG.CLINICAL.Application.Core.Units;

public interface IListUnitsService
{
    Task<Result<IReadOnlyList<UnitDto>>> ExecuteAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default);
}

public sealed class ListUnitsService : IListUnitsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IUnitsRepository _unitsRepository;

    public ListUnitsService(
        ICurrentTenantContext tenantContext,
        IUnitsRepository unitsRepository)
    {
        _tenantContext = tenantContext;
        _unitsRepository = unitsRepository;
    }

    public async Task<Result<IReadOnlyList<UnitDto>>> ExecuteAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var unidades = await _unitsRepository.ListByEmpresaIdAsync(
            _tenantContext.EmpresaId,
            includeInactive,
            cancellationToken);

        return Result<IReadOnlyList<UnitDto>>.Success(UnitsMapper.Map(unidades));
    }
}
