using BGD.CLINICAL.Application.Patients.Dtos;
using BGD.CLINICAL.Application.Patients.Patients;
using BGD.CLINICAL.WebApi.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BGD.CLINICAL.WebApi.Controllers.Patient;

[ApiController]
[Authorize]
[Route("api/patients")]
public sealed class PatientController : ControllerBase
{
    private readonly ICreatePatientsService _createPatientsService;
    private readonly IListPatientsService _listPatientsService;
    private readonly IGetPatientsService _getPatientsService;
    private readonly IUpdatePatientsService _updatePatientsService;
    private readonly IDeactivatePatientsService _deactivatePatientsService;
    private readonly IReactivatePatientsService _reactivatePatientsService;

    public PatientController(
        ICreatePatientsService createPatientsService,
        IListPatientsService listPatientsService,
        IGetPatientsService getPatientsService,
        IUpdatePatientsService updatePatientsService,
        IDeactivatePatientsService deactivatePatientsService,
        IReactivatePatientsService reactivatePatientsService)
    {
        _createPatientsService = createPatientsService;
        _listPatientsService = listPatientsService;
        _getPatientsService = getPatientsService;
        _updatePatientsService = updatePatientsService;
        _deactivatePatientsService = deactivatePatientsService;
        _reactivatePatientsService = reactivatePatientsService;
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] Guid? unidadeId = null,
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var result = await _listPatientsService.ExecuteAsync(unidadeId, includeInactive, cancellationToken);

        return Ok(new ApiResponse<IReadOnlyList<PatientDto>>(result.Value!, true));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getPatientsService.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<PatientDto>(result.Value!, true));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreatePatientRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createPatientsService.ExecuteAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        return CreatedAtAction(
            nameof(Get),
            new { id = result.Value!.Id },
            new ApiResponse<PatientDto>(result.Value, true));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdatePatientRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _updatePatientsService.ExecuteAsync(id, request, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Paciente não encontrado."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<PatientDto>(result.Value!, true));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _deactivatePatientsService.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Paciente não encontrado."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<PatientDto>(result.Value!, true));
    }

    [HttpPatch("{id:guid}/reactivate")]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _reactivatePatientsService.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Paciente não encontrado."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<PatientDto>(result.Value!, true));
    }
}
