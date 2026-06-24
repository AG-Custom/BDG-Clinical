using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Core.Abstractions;
using BGD.CLINICAL.Application.Core.Dtos;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;

namespace BGD.CLINICAL.Application.Core.Units;

public interface IDeactivateUnitsService
{
    Task<Result<UnitDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

public sealed class DeactivateUnitsService : IDeactivateUnitsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IUnitsRepository _unitsRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateUnitsService(
        ICurrentTenantContext tenantContext,
        IUnitsRepository unitsRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _unitsRepository = unitsRepository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UnitDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;
        var unidade = await _unitsRepository.GetByIdAndEmpresaIdAsync(id, empresaId, cancellationToken);

        if (unidade is null)
        {
            return Result<UnitDto>.Failure("Unidade não encontrada.");
        }

        if (!unidade.Ativo)
        {
            return Result<UnitDto>.Failure("Unidade já está inativa.");
        }

        var dadosAnteriores = UnitsAuditSerializer.Serialize(unidade);

        unidade.Deactivate();
        _unitsRepository.Update(unidade);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogsService.RegisterEntityChangeAsync(
            empresaId,
            _tenantContext.UsuarioId,
            nameof(Unidade),
            unidade.Id,
            AcaoAuditoria.Excluir,
            dadosAnteriores: dadosAnteriores,
            dadosNovos: UnitsAuditSerializer.Serialize(unidade),
            cancellationToken: cancellationToken);

        return Result<UnitDto>.Success(UnitsMapper.Map(unidade));
    }
}
