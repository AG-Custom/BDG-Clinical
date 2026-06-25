using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Application.Inventory.Dtos;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using BGD.CLINICAL.Domain.Exceptions;

namespace BGD.CLINICAL.Application.Inventory.MeasurementUnits;

public interface IUpdateMeasurementUnitsService
{
    Task<Result<MeasurementUnitDto>> ExecuteAsync(
        Guid id,
        UpdateMeasurementUnitRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class UpdateMeasurementUnitsService : IUpdateMeasurementUnitsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IMeasurementUnitsRepository _measurementUnitsRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateMeasurementUnitsService(
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
        UpdateMeasurementUnitRequest request,
        CancellationToken cancellationToken = default)
    {
        var nome = MeasurementUnitValidation.NormalizeNome(request.Nome);
        if (nome is null)
        {
            return Result<MeasurementUnitDto>.Failure("Informe o nome da unidade de medida.");
        }

        var sigla = MeasurementUnitValidation.NormalizeSigla(request.Sigla);
        if (sigla is null)
        {
            return Result<MeasurementUnitDto>.Failure("Informe a sigla da unidade de medida.");
        }

        if (sigla.Length > 30)
        {
            return Result<MeasurementUnitDto>.Failure("A sigla deve ter no máximo 30 caracteres.");
        }

        var tipoResult = MeasurementUnitValidation.ParseTipo(request.Tipo);
        if (tipoResult.IsFailure)
        {
            return Result<MeasurementUnitDto>.Failure(tipoResult.Error!);
        }

        var empresaId = _tenantContext.EmpresaId;
        var unidadeMedida = await _measurementUnitsRepository.GetByIdAndEmpresaIdAsync(id, empresaId, cancellationToken);

        if (unidadeMedida is null)
        {
            return Result<MeasurementUnitDto>.Failure("Unidade de medida não encontrada.");
        }

        if (!unidadeMedida.Ativo)
        {
            return Result<MeasurementUnitDto>.Failure("Não é possível editar uma unidade de medida inativa.");
        }

        if (await _measurementUnitsRepository.ExistsByNomeAsync(empresaId, nome, id, cancellationToken))
        {
            return Result<MeasurementUnitDto>.Failure("Já existe uma unidade de medida com este nome.");
        }

        if (await _measurementUnitsRepository.ExistsBySiglaAsync(empresaId, sigla, id, cancellationToken))
        {
            return Result<MeasurementUnitDto>.Failure("Já existe uma unidade de medida com esta sigla.");
        }

        try
        {
            var dadosAnteriores = MeasurementUnitsAuditSerializer.Serialize(unidadeMedida);

            unidadeMedida.UpdateDetails(nome, sigla, tipoResult.Value!);
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
        catch (DomainException exception)
        {
            return Result<MeasurementUnitDto>.Failure(exception.Message);
        }
    }
}
