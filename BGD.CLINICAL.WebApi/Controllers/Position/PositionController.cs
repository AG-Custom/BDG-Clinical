using BGD.CLINICAL.Application.Core.Dtos;
using BGD.CLINICAL.Application.Core.Positions;
using BGD.CLINICAL.WebApi.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BGD.CLINICAL.WebApi.Controllers.Position;

[ApiController]
[Authorize]
[Route("api/positions")]
public sealed class PositionController : ControllerBase
{
    private readonly ICreatePositionsService _createPositionsService;
    private readonly IListPositionsService _listPositionsService;
    private readonly IGetPositionsService _getPositionsService;
    private readonly IUpdatePositionsService _updatePositionsService;
    private readonly IDeactivatePositionsService _deactivatePositionsService;
    private readonly IReactivatePositionsService _reactivatePositionsService;

    public PositionController(
        ICreatePositionsService createPositionsService,
        IListPositionsService listPositionsService,
        IGetPositionsService getPositionsService,
        IUpdatePositionsService updatePositionsService,
        IDeactivatePositionsService deactivatePositionsService,
        IReactivatePositionsService reactivatePositionsService)
    {
        _createPositionsService = createPositionsService;
        _listPositionsService = listPositionsService;
        _getPositionsService = getPositionsService;
        _updatePositionsService = updatePositionsService;
        _deactivatePositionsService = deactivatePositionsService;
        _reactivatePositionsService = reactivatePositionsService;
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var result = await _listPositionsService.ExecuteAsync(includeInactive, cancellationToken);

        return Ok(new ApiResponse<IReadOnlyList<PositionDto>>(result.Value!, true));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getPositionsService.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<PositionDto>(result.Value!, true));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreatePositionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createPositionsService.ExecuteAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        return CreatedAtAction(
            nameof(Get),
            new { id = result.Value!.Id },
            new ApiResponse<PositionDto>(result.Value, true));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdatePositionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _updatePositionsService.ExecuteAsync(id, request, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Cargo não encontrado."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<PositionDto>(result.Value!, true));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _deactivatePositionsService.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Cargo não encontrado."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<PositionDto>(result.Value!, true));
    }

    [HttpPatch("{id:guid}/reactivate")]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _reactivatePositionsService.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Cargo não encontrado."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<PositionDto>(result.Value!, true));
    }
}
