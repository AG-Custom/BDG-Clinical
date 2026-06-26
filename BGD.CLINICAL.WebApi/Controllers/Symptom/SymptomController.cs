using BGD.CLINICAL.Application.Patients.Dtos;
using BGD.CLINICAL.Application.Patients.Symptoms;
using BGD.CLINICAL.WebApi.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BGD.CLINICAL.WebApi.Controllers.Symptom;

[ApiController]
[Authorize]
[Route("api/symptoms")]
public sealed class SymptomController : ControllerBase
{
    private readonly ICreateSymptomsService _createSymptomsService;
    private readonly IListSymptomsService _listSymptomsService;
    private readonly IGetSymptomsService _getSymptomsService;
    private readonly IUpdateSymptomsService _updateSymptomsService;
    private readonly IDeactivateSymptomsService _deactivateSymptomsService;
    private readonly IReactivateSymptomsService _reactivateSymptomsService;

    public SymptomController(
        ICreateSymptomsService createSymptomsService,
        IListSymptomsService listSymptomsService,
        IGetSymptomsService getSymptomsService,
        IUpdateSymptomsService updateSymptomsService,
        IDeactivateSymptomsService deactivateSymptomsService,
        IReactivateSymptomsService reactivateSymptomsService)
    {
        _createSymptomsService = createSymptomsService;
        _listSymptomsService = listSymptomsService;
        _getSymptomsService = getSymptomsService;
        _updateSymptomsService = updateSymptomsService;
        _deactivateSymptomsService = deactivateSymptomsService;
        _reactivateSymptomsService = reactivateSymptomsService;
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var result = await _listSymptomsService.ExecuteAsync(includeInactive, cancellationToken);

        return Ok(new ApiResponse<IReadOnlyList<SymptomDto>>(result.Value!, true));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getSymptomsService.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<SymptomDto>(result.Value!, true));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateSymptomRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createSymptomsService.ExecuteAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        return CreatedAtAction(
            nameof(Get),
            new { id = result.Value!.Id },
            new ApiResponse<SymptomDto>(result.Value, true));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateSymptomRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _updateSymptomsService.ExecuteAsync(id, request, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Sintoma não encontrado."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<SymptomDto>(result.Value!, true));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _deactivateSymptomsService.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Sintoma não encontrado."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<SymptomDto>(result.Value!, true));
    }

    [HttpPatch("{id:guid}/reactivate")]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _reactivateSymptomsService.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Sintoma não encontrado."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<SymptomDto>(result.Value!, true));
    }
}
