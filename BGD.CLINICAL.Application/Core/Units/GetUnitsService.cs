using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Core.Abstractions;
using BGD.CLINICAL.Application.Core.Dtos;

namespace BGD.CLINICAL.Application.Core.Units;

public interface IGetUnitsService
{
    Task<Result<UnitDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

public sealed class GetUnitsService : IGetUnitsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IUnitsRepository _unitsRepository;

    public GetUnitsService(
        ICurrentTenantContext tenantContext,
        IUnitsRepository unitsRepository)
    {
        _tenantContext = tenantContext;
        _unitsRepository = unitsRepository;
    }

    public async Task<Result<UnitDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var unidade = await _unitsRepository.GetByIdAndEmpresaIdAsync(
            id,
            _tenantContext.EmpresaId,
            cancellationToken);

        if (unidade is null)
        {
            return Result<UnitDto>.Failure("Unidade não encontrada.");
        }

        return Result<UnitDto>.Success(UnitsMapper.Map(unidade));
    }
}
