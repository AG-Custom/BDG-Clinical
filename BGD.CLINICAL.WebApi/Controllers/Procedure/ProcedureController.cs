using BGD.CLINICAL.Application.Applications.Dtos;
using BGD.CLINICAL.Application.Applications.Procedures;
using BGD.CLINICAL.WebApi.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BGD.CLINICAL.WebApi.Controllers.Procedure;

[ApiController]
[Authorize]
[Route("api/procedures")]
public sealed class ProcedureController : ControllerBase
{
    private readonly ICreateProceduresService _createProceduresService;
    private readonly IListProceduresService _listProceduresService;
    private readonly IGetProceduresService _getProceduresService;
    private readonly IUpdateProceduresService _updateProceduresService;
    private readonly IDeactivateProceduresService _deactivateProceduresService;
    private readonly IReactivateProceduresService _reactivateProceduresService;

    public ProcedureController(
        ICreateProceduresService createProceduresService,
        IListProceduresService listProceduresService,
        IGetProceduresService getProceduresService,
        IUpdateProceduresService updateProceduresService,
        IDeactivateProceduresService deactivateProceduresService,
        IReactivateProceduresService reactivateProceduresService)
    {
        _createProceduresService = createProceduresService;
        _listProceduresService = listProceduresService;
        _getProceduresService = getProceduresService;
        _updateProceduresService = updateProceduresService;
        _deactivateProceduresService = deactivateProceduresService;
        _reactivateProceduresService = reactivateProceduresService;
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] bool includeInactive = false,
        [FromQuery] Guid? produtoAplicadoId = null,
        [FromQuery] string? search = null,
        [FromQuery] int? limit = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _listProceduresService.ExecuteAsync(
            includeInactive,
            search,
            produtoAplicadoId,
            limit,
            cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<IReadOnlyList<ProcedureDto>>(result.Value!, true));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getProceduresService.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<ProcedureDto>(result.Value!, true));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateProcedureRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createProceduresService.ExecuteAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        return CreatedAtAction(
            nameof(Get),
            new { id = result.Value!.Id },
            new ApiResponse<ProcedureDto>(result.Value, true));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateProcedureRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _updateProceduresService.ExecuteAsync(id, request, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Procedimento não encontrado."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<ProcedureDto>(result.Value!, true));
    }

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _deactivateProceduresService.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Procedimento não encontrado."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<ProcedureDto>(result.Value!, true));
    }

    [HttpPatch("{id:guid}/reactivate")]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _reactivateProceduresService.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Procedimento não encontrado."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<ProcedureDto>(result.Value!, true));
    }
}
