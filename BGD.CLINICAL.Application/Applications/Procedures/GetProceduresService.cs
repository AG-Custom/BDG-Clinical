using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Applications.Abstractions;
using BGD.CLINICAL.Application.Applications.Dtos;
using BGD.CLINICAL.Application.Common;

namespace BGD.CLINICAL.Application.Applications.Procedures;

public interface IGetProceduresService
{
    Task<Result<ProcedureDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

public sealed class GetProceduresService : IGetProceduresService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IProceduresRepository _proceduresRepository;

    public GetProceduresService(
        ICurrentTenantContext tenantContext,
        IProceduresRepository proceduresRepository)
    {
        _tenantContext = tenantContext;
        _proceduresRepository = proceduresRepository;
    }

    public async Task<Result<ProcedureDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var procedimento = await _proceduresRepository.GetByIdAndEmpresaIdWithDetailsAsync(
            id,
            _tenantContext.EmpresaId,
            cancellationToken);

        if (procedimento is null)
        {
            return Result<ProcedureDto>.Failure("Procedimento não encontrado.");
        }

        return Result<ProcedureDto>.Success(ProceduresMapper.Map(procedimento));
    }
}
