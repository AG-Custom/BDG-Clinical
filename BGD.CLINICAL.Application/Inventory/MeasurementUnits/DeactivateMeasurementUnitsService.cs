using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Application.Inventory.Dtos;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;

namespace BGD.CLINICAL.Application.Inventory.MeasurementUnits;

public interface IDeactivateMeasurementUnitsService
{
    Task<Result<MeasurementUnitDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

public sealed class DeactivateMeasurementUnitsService : IDeactivateMeasurementUnitsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IMeasurementUnitsRepository _measurementUnitsRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateMeasurementUnitsService(
        ICurrentTenantContext tenantContext,
        IMeasurementUnitsRepository measurementUnitsRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _measurementUnitsRepository = measurementUnitsRepository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<MeasurementUnitDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;
        var unidadeMedida = await _measurementUnitsRepository.GetByIdAndEmpresaIdAsync(id, empresaId, cancellationToken);

        if (unidadeMedida is null)
        {
            return Result<MeasurementUnitDto>.Failure("Unidade de medida não encontrada.");
        }

        if (!unidadeMedida.Ativo)
        {
            return Result<MeasurementUnitDto>.Failure("Unidade de medida já está inativa.");
        }

        if (await _measurementUnitsRepository.HasActiveProductsAsync(id, empresaId, cancellationToken))
        {
            return Result<MeasurementUnitDto>.Failure(
                "Não é possível inativar uma unidade de medida vinculada a produtos ativos.");
        }

        var dadosAnteriores = MeasurementUnitsAuditSerializer.Serialize(unidadeMedida);

        unidadeMedida.Deactivate();
        _measurementUnitsRepository.Update(unidadeMedida);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogsService.RegisterEntityChangeAsync(
            empresaId,
            _tenantContext.UsuarioId,
            nameof(UnidadeMedida),
            unidadeMedida.Id,
            AcaoAuditoria.Excluir,
            dadosAnteriores: dadosAnteriores,
            dadosNovos: MeasurementUnitsAuditSerializer.Serialize(unidadeMedida),
            cancellationToken: cancellationToken);

        return Result<MeasurementUnitDto>.Success(MeasurementUnitsMapper.Map(unidadeMedida));
    }
}
