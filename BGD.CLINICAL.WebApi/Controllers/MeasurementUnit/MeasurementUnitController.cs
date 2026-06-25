using BGD.CLINICAL.Application.Inventory.Dtos;
using BGD.CLINICAL.Application.Inventory.MeasurementUnits;
using BGD.CLINICAL.WebApi.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BGD.CLINICAL.WebApi.Controllers.MeasurementUnit;

[ApiController]
[Authorize]
[Route("api/measurement-units")]
public sealed class MeasurementUnitController : ControllerBase
{
    private readonly ICreateMeasurementUnitsService _createMeasurementUnitsService;
    private readonly IListMeasurementUnitsService _listMeasurementUnitsService;
    private readonly IGetMeasurementUnitsService _getMeasurementUnitsService;
    private readonly IUpdateMeasurementUnitsService _updateMeasurementUnitsService;
    private readonly IDeactivateMeasurementUnitsService _deactivateMeasurementUnitsService;
    private readonly IReactivateMeasurementUnitsService _reactivateMeasurementUnitsService;

    public MeasurementUnitController(
        ICreateMeasurementUnitsService createMeasurementUnitsService,
        IListMeasurementUnitsService listMeasurementUnitsService,
        IGetMeasurementUnitsService getMeasurementUnitsService,
        IUpdateMeasurementUnitsService updateMeasurementUnitsService,
        IDeactivateMeasurementUnitsService deactivateMeasurementUnitsService,
        IReactivateMeasurementUnitsService reactivateMeasurementUnitsService)
    {
        _createMeasurementUnitsService = createMeasurementUnitsService;
        _listMeasurementUnitsService = listMeasurementUnitsService;
        _getMeasurementUnitsService = getMeasurementUnitsService;
        _updateMeasurementUnitsService = updateMeasurementUnitsService;
        _deactivateMeasurementUnitsService = deactivateMeasurementUnitsService;
        _reactivateMeasurementUnitsService = reactivateMeasurementUnitsService;
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] bool includeInactive = false,
        [FromQuery] string? tipo = null,
        [FromQuery] string? search = null,
        [FromQuery] int? limit = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _listMeasurementUnitsService.ExecuteAsync(
            includeInactive,
            tipo,
            search,
            limit,
            cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<IReadOnlyList<MeasurementUnitDto>>(result.Value!, true));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getMeasurementUnitsService.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<MeasurementUnitDto>(result.Value!, true));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateMeasurementUnitRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createMeasurementUnitsService.ExecuteAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        return CreatedAtAction(
            nameof(Get),
            new { id = result.Value!.Id },
            new ApiResponse<MeasurementUnitDto>(result.Value, true));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateMeasurementUnitRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _updateMeasurementUnitsService.ExecuteAsync(id, request, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Unidade de medida não encontrada."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<MeasurementUnitDto>(result.Value!, true));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _deactivateMeasurementUnitsService.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Unidade de medida não encontrada."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<MeasurementUnitDto>(result.Value!, true));
    }

    [HttpPatch("{id:guid}/reactivate")]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _reactivateMeasurementUnitsService.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Unidade de medida não encontrada."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<MeasurementUnitDto>(result.Value!, true));
    }
}
