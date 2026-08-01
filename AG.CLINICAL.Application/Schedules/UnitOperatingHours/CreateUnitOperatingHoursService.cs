using AG.CLINICAL.Application.Abstractions.Persistence;
using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Core.Abstractions;
using AG.CLINICAL.Application.Schedules.Abstractions;
using AG.CLINICAL.Application.Schedules.Dtos;
using AG.CLINICAL.Domain.Entities;
using AG.CLINICAL.Domain.Exceptions;

namespace AG.CLINICAL.Application.Schedules.UnitOperatingHours;

public interface ICreateUnitOperatingHoursService
{
    Task<Result<UnitOperatingHourDto>> ExecuteAsync(
        Guid unitId,
        CreateUnitOperatingHourRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class CreateUnitOperatingHoursService : ICreateUnitOperatingHoursService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IUnitsRepository _unitsRepository;
    private readonly IUnitOperatingHoursRepository _operatingHoursRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateUnitOperatingHoursService(
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
        CreateUnitOperatingHourRequest request,
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
            excludeId: null,
            cancellationToken: cancellationToken);

        if (overlapValidation.IsFailure)
        {
            return Result<UnitOperatingHourDto>.Failure(overlapValidation.Error!);
        }

        try
        {
            var horario = new HorarioFuncionamentoUnidade(
                empresaId,
                unitId,
                diaSemanaResult.Value!,
                request.HoraInicio,
                request.HoraFim);

            await _operatingHoursRepository.AddAsync(horario, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<UnitOperatingHourDto>.Success(UnitOperatingHoursMapper.Map(horario));
        }
        catch (DomainException exception)
        {
            return Result<UnitOperatingHourDto>.Failure(exception.Message);
        }
    }
}
