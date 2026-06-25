using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Core.Abstractions;
using BGD.CLINICAL.Application.Core.Dtos;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;

namespace BGD.CLINICAL.Application.Core.Positions;

public interface IDeactivatePositionsService
{
    Task<Result<PositionDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

public sealed class DeactivatePositionsService : IDeactivatePositionsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IPositionsRepository _positionsRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivatePositionsService(
        ICurrentTenantContext tenantContext,
        IPositionsRepository positionsRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _positionsRepository = positionsRepository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PositionDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;
        var cargo = await _positionsRepository.GetByIdAndEmpresaIdAsync(id, empresaId, cancellationToken);

        if (cargo is null)
        {
            return Result<PositionDto>.Failure("Cargo não encontrado.");
        }

        if (!cargo.Ativo)
        {
            return Result<PositionDto>.Failure("Cargo já está inativo.");
        }

        var dadosAnteriores = PositionsAuditSerializer.Serialize(cargo);

        cargo.Deactivate();
        _positionsRepository.Update(cargo);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogsService.RegisterEntityChangeAsync(
            empresaId,
            _tenantContext.UsuarioId,
            nameof(Cargo),
            cargo.Id,
            AcaoAuditoria.Excluir,
            dadosAnteriores: dadosAnteriores,
            dadosNovos: PositionsAuditSerializer.Serialize(cargo),
            cancellationToken: cancellationToken);

        return Result<PositionDto>.Success(PositionsMapper.Map(cargo));
    }
}
