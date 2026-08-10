using AG.CLINICAL.Application.Abstractions.Persistence;
using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Core.Abstractions;
using AG.CLINICAL.Application.Schedules.Abstractions;
using AG.CLINICAL.Application.Schedules.Dtos;
using AG.CLINICAL.Domain.Exceptions;

namespace AG.CLINICAL.Application.Schedules.UnitOperatingHours;

public interface IUpdateUnitOperatingHoursService
{
    Task<Result<UnitOperatingHourDto>> ExecuteAsync(
        Guid unitId,
        Guid id,
        UpdateUnitOperatingHourRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class UpdateUnitOperatingHoursService : IUpdateUnitOperatingHoursService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IUnitsRepository _unitsRepository;
    private readonly IUnitOperatingHoursRepository _operatingHoursRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUnitOperatingHoursService(
        ICurrentTenantContext tenantContext,
        IUnitsRepository unitsRepository,
        IUnitOperatingHoursRepository operatingHoursRepository,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _unitsRepository = unitsRepository;
        _operatingHoursRepository = operatingHoursRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UnitOperatingHourDto>> ExecuteAsync(
        Guid unitId,
        Guid id,
        UpdateUnitOperatingHourRequest request,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;

        var unitValidation = await UnitOperatingHoursValidator.EnsureUnitExistsAsync(
            empresaId,
            unitId,
            _unitsRepository,
            cancellationToken);

        if (unitValidation.IsFailure)
        {
            return Result<UnitOperatingHourDto>.Failure(unitValidation.Error!);
        }

        var horario = await _operatingHoursRepository.GetByIdAndEmpresaIdAsync(id, empresaId, cancellationToken);
        if (horario is null || horario.UnidadeId != unitId)
        {
            return Result<UnitOperatingHourDto>.Failure("Horário de funcionamento não encontrado.");
        }

        var diaSemanaResult = UnitOperatingHoursValidator.ParseDiaSemana(request.DiaSemana);
        if (diaSemanaResult.IsFailure)
        {
            return Result<UnitOperatingHourDto>.Failure(diaSemanaResult.Error!);
        }

        var overlapValidation = await UnitOperatingHoursValidator.EnsureNoOverlapAsync(
            empresaId,
            unitId,
            diaSemanaResult.Value!,
            request.HoraInicio,
            request.HoraFim,
            _operatingHoursRepository,
            excludeId: id,
            cancellationToken: cancellationToken);

        if (overlapValidation.IsFailure)
        {
            return Result<UnitOperatingHourDto>.Failure(overlapValidation.Error!);
        }

        try
        {
            horario.UpdateDetails(
                diaSemanaResult.Value!,
                request.HoraInicio,
                request.HoraFim,
                request.Ativo);

            _operatingHoursRepository.Update(horario);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<UnitOperatingHourDto>.Success(UnitOperatingHoursMapper.Map(horario));
        }
        catch (DomainException exception)
        {
            return Result<UnitOperatingHourDto>.Failure(exception.Message);
        }
    }
}
