using BGD.CLINICAL.Application.Core.Dtos;
using BGD.CLINICAL.Application.Core.PositionPermissions;
using BGD.CLINICAL.Application.Core.Positions;
using BGD.CLINICAL.Application.Modules.Dtos;
using BGD.CLINICAL.WebApi.Authorization;
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
    private readonly IGetPositionPermissionsService _getPositionPermissionsService;
    private readonly IUpdatePositionPermissionsService _updatePositionPermissionsService;

    public PositionController(
        ICreatePositionsService createPositionsService,
        IListPositionsService listPositionsService,
        IGetPositionsService getPositionsService,
        IUpdatePositionsService updatePositionsService,
        IDeactivatePositionsService deactivatePositionsService,
        IReactivatePositionsService reactivatePositionsService,
        IGetPositionPermissionsService getPositionPermissionsService,
        IUpdatePositionPermissionsService updatePositionPermissionsService)
    {
        _createPositionsService = createPositionsService;
        _listPositionsService = listPositionsService;
        _getPositionsService = getPositionsService;
        _updatePositionsService = updatePositionsService;
        _deactivatePositionsService = deactivatePositionsService;
        _reactivatePositionsService = reactivatePositionsService;
        _getPositionPermissionsService = getPositionPermissionsService;
        _updatePositionPermissionsService = updatePositionPermissionsService;
    }

    [HttpGet]
    [RequireAnyPermissionFrom(AuxiliaryPermissionSet.Positions)]
    public async Task<IActionResult> List(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var result = await _listPositionsService.ExecuteAsync(includeInactive, cancellationToken);

        return Ok(new ApiResponse<IReadOnlyList<PositionDto>>(result.Value!, true));
    }

    [HttpGet("{id:guid}")]
    [RequireAnyPermissionFrom(AuxiliaryPermissionSet.Positions)]
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
    [RequirePermission("funcionario.editar")]
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
    [RequirePermission("funcionario.editar")]
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
    [RequirePermission("funcionario.editar")]
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
    [RequirePermission("funcionario.editar")]
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

    [HttpGet("{id:guid}/permissions")]
    [RequirePermission("funcionario.visualizar")]
    public async Task<IActionResult> GetPermissions(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getPositionPermissionsService.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<PositionPermissionsDto>(result.Value!, true));
    }

    [HttpPut("{id:guid}/permissions")]
    [RequirePermission("funcionario.editar")]
    public async Task<IActionResult> UpdatePermissions(
        Guid id,
        [FromBody] UpdatePositionPermissionsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _updatePositionPermissionsService.ExecuteAsync(id, request, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Cargo não encontrado."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<PositionPermissionsDto>(result.Value!, true));
    }
}
