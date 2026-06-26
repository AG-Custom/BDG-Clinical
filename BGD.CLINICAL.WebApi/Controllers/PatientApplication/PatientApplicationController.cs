using BGD.CLINICAL.Application.Applications.Dtos;
using BGD.CLINICAL.Application.Applications.PatientApplications;
using BGD.CLINICAL.WebApi.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BGD.CLINICAL.WebApi.Controllers.PatientApplication;

[ApiController]
[Authorize]
[Route("api/patient-applications")]
public sealed class PatientApplicationController : ControllerBase
{
    private readonly ICreatePatientApplicationsService _createPatientApplicationsService;
    private readonly IListPatientApplicationsService _listPatientApplicationsService;
    private readonly IGetPatientApplicationsService _getPatientApplicationsService;
    private readonly IUpdatePatientApplicationsService _updatePatientApplicationsService;
    private readonly ICancelPatientApplicationsService _cancelPatientApplicationsService;

    public PatientApplicationController(
        ICreatePatientApplicationsService createPatientApplicationsService,
        IListPatientApplicationsService listPatientApplicationsService,
        IGetPatientApplicationsService getPatientApplicationsService,
        IUpdatePatientApplicationsService updatePatientApplicationsService,
        ICancelPatientApplicationsService cancelPatientApplicationsService)
    {
        _createPatientApplicationsService = createPatientApplicationsService;
        _listPatientApplicationsService = listPatientApplicationsService;
        _getPatientApplicationsService = getPatientApplicationsService;
        _updatePatientApplicationsService = updatePatientApplicationsService;
        _cancelPatientApplicationsService = cancelPatientApplicationsService;
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] Guid? pacienteId = null,
        [FromQuery] Guid? unidadeId = null,
        [FromQuery] Guid? produtoId = null,
        [FromQuery] Guid? procedimentoId = null,
        [FromQuery] Guid? aplicadorId = null,
        [FromQuery] bool? cancelada = null,
        [FromQuery] DateTime? dataInicio = null,
        [FromQuery] DateTime? dataFim = null,
        [FromQuery] int? limit = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _listPatientApplicationsService.ExecuteAsync(
            pacienteId,
            unidadeId,
            produtoId,
            procedimentoId,
            aplicadorId,
            cancelada,
            dataInicio,
            dataFim,
            limit,
            cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<IReadOnlyList<PatientApplicationDto>>(result.Value!, true));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getPatientApplicationsService.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<PatientApplicationDto>(result.Value!, true));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreatePatientApplicationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createPatientApplicationsService.ExecuteAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        return CreatedAtAction(
            nameof(Get),
            new { id = result.Value!.Id },
            new ApiResponse<PatientApplicationDto>(result.Value, true));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdatePatientApplicationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _updatePatientApplicationsService.ExecuteAsync(id, request, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<PatientApplicationDto>(result.Value!, true));
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var result = await _cancelPatientApplicationsService.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<PatientApplicationDto>(result.Value!, true));
    }
}
