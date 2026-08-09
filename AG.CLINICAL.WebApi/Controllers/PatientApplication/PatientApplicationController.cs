using AG.CLINICAL.Application.Applications.Dtos;
using AG.CLINICAL.Application.Applications.PatientApplications;
using AG.CLINICAL.WebApi.Authorization;
using AG.CLINICAL.WebApi.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AG.CLINICAL.WebApi.Controllers.PatientApplication;

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
    [RequirePermission("aplicacao.visualizar")]
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
    [RequirePermission("aplicacao.visualizar")]
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
    [RequirePermission("aplicacao.criar")]
    public async Task<IActionResult> Create(
        [FromBody] CreatePatientApplicationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createPatientApplicationsService.ExecuteAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        var aplicacoes = result.Value!.Aplicacoes;
        var primeiroId = aplicacoes.Count > 0 ? aplicacoes[0].Id : Guid.Empty;

        return CreatedAtAction(
            nameof(Get),
            new { id = primeiroId },
            new ApiResponse<CreatePatientApplicationsResult>(result.Value, true));
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("aplicacao.editar")]
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
    [RequirePermission("aplicacao.cancelar")]
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
