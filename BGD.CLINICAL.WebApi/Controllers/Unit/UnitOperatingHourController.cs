using BGD.CLINICAL.Application.Schedules.Dtos;
using BGD.CLINICAL.Application.Schedules.UnitOperatingHours;
using BGD.CLINICAL.WebApi.Authorization;
using BGD.CLINICAL.WebApi.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BGD.CLINICAL.WebApi.Controllers.Unit;

[ApiController]
[Authorize]
[Route("api/units/{unitId:guid}/operating-hours")]
public sealed class UnitOperatingHourController : ControllerBase
{
    private readonly IListUnitOperatingHoursService _listUnitOperatingHoursService;
    private readonly ICreateUnitOperatingHoursService _createUnitOperatingHoursService;
    private readonly IUpdateUnitOperatingHoursService _updateUnitOperatingHoursService;

    public UnitOperatingHourController(
        IListUnitOperatingHoursService listUnitOperatingHoursService,
        ICreateUnitOperatingHoursService createUnitOperatingHoursService,
        IUpdateUnitOperatingHoursService updateUnitOperatingHoursService)
    {
        _listUnitOperatingHoursService = listUnitOperatingHoursService;
        _createUnitOperatingHoursService = createUnitOperatingHoursService;
        _updateUnitOperatingHoursService = updateUnitOperatingHoursService;
    }

    [HttpGet]
    [RequireAnyPermissionFrom(AuxiliaryPermissionSet.OperatingHours)]
    public async Task<IActionResult> List(
        Guid unitId,
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var result = await _listUnitOperatingHoursService.ExecuteAsync(unitId, includeInactive, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<IReadOnlyList<UnitOperatingHourDto>>(result.Value!, true));
    }

    [HttpPost]
    [RequirePermission("unidade.editar")]
    public async Task<IActionResult> Create(
        Guid unitId,
        [FromBody] CreateUnitOperatingHourRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createUnitOperatingHoursService.ExecuteAsync(unitId, request, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        return CreatedAtAction(
            nameof(List),
            new { unitId },
            new ApiResponse<UnitOperatingHourDto>(result.Value!, true));
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("unidade.editar")]
    public async Task<IActionResult> Update(
        Guid unitId,
        Guid id,
        [FromBody] UpdateUnitOperatingHourRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _updateUnitOperatingHoursService.ExecuteAsync(unitId, id, request, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Horário de funcionamento não encontrado."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<UnitOperatingHourDto>(result.Value!, true));
    }
}
