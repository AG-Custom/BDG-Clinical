using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Core.Abstractions;
using BGD.CLINICAL.Application.Core.Dtos;

namespace BGD.CLINICAL.Application.Core.Positions;

public interface IListPositionsService
{
    Task<Result<IReadOnlyList<PositionDto>>> ExecuteAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default);
}

public sealed class ListPositionsService : IListPositionsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IPositionsRepository _positionsRepository;

    public ListPositionsService(
        ICurrentTenantContext tenantContext,
        IPositionsRepository positionsRepository)
    {
        _tenantContext = tenantContext;
        _positionsRepository = positionsRepository;
    }

    public async Task<Result<IReadOnlyList<PositionDto>>> ExecuteAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var cargos = await _positionsRepository.ListByEmpresaIdAsync(
            _tenantContext.EmpresaId,
            includeInactive,
            cancellationToken);

        return Result<IReadOnlyList<PositionDto>>.Success(PositionsMapper.Map(cargos));
    }
}
