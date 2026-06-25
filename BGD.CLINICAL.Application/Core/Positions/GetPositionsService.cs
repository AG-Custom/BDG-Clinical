using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Core.Abstractions;
using BGD.CLINICAL.Application.Core.Dtos;

namespace BGD.CLINICAL.Application.Core.Positions;

public interface IGetPositionsService
{
    Task<Result<PositionDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

public sealed class GetPositionsService : IGetPositionsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IPositionsRepository _positionsRepository;

    public GetPositionsService(
        ICurrentTenantContext tenantContext,
        IPositionsRepository positionsRepository)
    {
        _tenantContext = tenantContext;
        _positionsRepository = positionsRepository;
    }

    public async Task<Result<PositionDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var cargo = await _positionsRepository.GetByIdAndEmpresaIdAsync(
            id,
            _tenantContext.EmpresaId,
            cancellationToken);

        if (cargo is null)
        {
            return Result<PositionDto>.Failure("Cargo não encontrado.");
        }

        return Result<PositionDto>.Success(PositionsMapper.Map(cargo));
    }
}
