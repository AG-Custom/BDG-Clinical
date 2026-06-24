using BGD.CLINICAL.Application.Core.Dtos;
using BGD.CLINICAL.Application.Core.Units;
using BGD.CLINICAL.WebApi.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BGD.CLINICAL.WebApi.Controllers.Unit;

[ApiController]
[Authorize]
[Route("api/units")]
public sealed class UnitController : ControllerBase
{
    private readonly ICreateUnitsService _createUnitsService;
    private readonly IListUnitsService _listUnitsService;
    private readonly IGetUnitsService _getUnitsService;
    private readonly IUpdateUnitsService _updateUnitsService;
    private readonly IDeactivateUnitsService _deactivateUnitsService;

    public UnitController(
        ICreateUnitsService createUnitsService,
        IListUnitsService listUnitsService,
        IGetUnitsService getUnitsService,
        IUpdateUnitsService updateUnitsService,
        IDeactivateUnitsService deactivateUnitsService)
    {
        _createUnitsService = createUnitsService;
        _listUnitsService = listUnitsService;
        _getUnitsService = getUnitsService;
        _updateUnitsService = updateUnitsService;
        _deactivateUnitsService = deactivateUnitsService;
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var result = await _listUnitsService.ExecuteAsync(includeInactive, cancellationToken);

        return Ok(new ApiResponse<IReadOnlyList<UnitDto>>(result.Value!, true));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getUnitsService.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<UnitDto>(result.Value!, true));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateUnitRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createUnitsService.ExecuteAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        return CreatedAtAction(
            nameof(Get),
            new { id = result.Value!.Id },
            new ApiResponse<UnitDto>(result.Value, true));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateUnitRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _updateUnitsService.ExecuteAsync(id, request, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Unidade não encontrada."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<UnitDto>(result.Value!, true));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _deactivateUnitsService.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Unidade não encontrada."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<UnitDto>(result.Value!, true));
    }
}
