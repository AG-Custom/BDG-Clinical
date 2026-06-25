using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Application.Inventory.Dtos;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;

namespace BGD.CLINICAL.Application.Inventory.MeasurementUnits;

public interface IReactivateMeasurementUnitsService
{
    Task<Result<MeasurementUnitDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

public sealed class ReactivateMeasurementUnitsService : IReactivateMeasurementUnitsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IMeasurementUnitsRepository _measurementUnitsRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public ReactivateMeasurementUnitsService(
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

        if (unidadeMedida.Ativo)
        {
            return Result<MeasurementUnitDto>.Failure("Unidade de medida já está ativa.");
        }

        if (await _measurementUnitsRepository.ExistsByNomeAsync(empresaId, unidadeMedida.Nome, id, cancellationToken))
        {
            return Result<MeasurementUnitDto>.Failure("Já existe uma unidade de medida ativa com este nome.");
        }

        if (await _measurementUnitsRepository.ExistsBySiglaAsync(empresaId, unidadeMedida.Sigla, id, cancellationToken))
        {
            return Result<MeasurementUnitDto>.Failure("Já existe uma unidade de medida ativa com esta sigla.");
        }

        var dadosAnteriores = MeasurementUnitsAuditSerializer.Serialize(unidadeMedida);

        unidadeMedida.Reactivate();
        _measurementUnitsRepository.Update(unidadeMedida);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogsService.RegisterEntityChangeAsync(
            empresaId,
            _tenantContext.UsuarioId,
            nameof(UnidadeMedida),
            unidadeMedida.Id,
            AcaoAuditoria.Editar,
            dadosAnteriores: dadosAnteriores,
            dadosNovos: MeasurementUnitsAuditSerializer.Serialize(unidadeMedida),
            cancellationToken: cancellationToken);

        return Result<MeasurementUnitDto>.Success(MeasurementUnitsMapper.Map(unidadeMedida));
    }
}
