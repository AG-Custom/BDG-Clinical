using BGD.CLINICAL.Application.Packages.Dtos;
using BGD.CLINICAL.Application.Packages.PatientPurchases;
using BGD.CLINICAL.WebApi.Authorization;
using BGD.CLINICAL.WebApi.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BGD.CLINICAL.WebApi.Controllers.PatientPurchase;

[ApiController]
[Authorize]
public sealed class PatientPurchaseController : ControllerBase
{
    private readonly ICreatePatientPurchasesService _createPatientPurchasesService;
    private readonly IListAllPatientPurchasesService _listAllPatientPurchasesService;
    private readonly IListPatientPurchasesService _listPatientPurchasesService;
    private readonly IListActivePatientPurchasesService _listActivePatientPurchasesService;
    private readonly IGetPatientPurchasesService _getPatientPurchasesService;
    private readonly IGetPatientPurchaseBalanceService _getPatientPurchaseBalanceService;
    private readonly ICancelPatientPurchasesService _cancelPatientPurchasesService;

    public PatientPurchaseController(
        ICreatePatientPurchasesService createPatientPurchasesService,
        IListAllPatientPurchasesService listAllPatientPurchasesService,
        IListPatientPurchasesService listPatientPurchasesService,
        IListActivePatientPurchasesService listActivePatientPurchasesService,
        IGetPatientPurchasesService getPatientPurchasesService,
        IGetPatientPurchaseBalanceService getPatientPurchaseBalanceService,
        ICancelPatientPurchasesService cancelPatientPurchasesService)
    {
        _createPatientPurchasesService = createPatientPurchasesService;
        _listAllPatientPurchasesService = listAllPatientPurchasesService;
        _listPatientPurchasesService = listPatientPurchasesService;
        _listActivePatientPurchasesService = listActivePatientPurchasesService;
        _getPatientPurchasesService = getPatientPurchasesService;
        _getPatientPurchaseBalanceService = getPatientPurchaseBalanceService;
        _cancelPatientPurchasesService = cancelPatientPurchasesService;
    }

    [HttpGet("api/patient-purchases")]
    [RequireAnyPermissionFrom(AuxiliaryPermissionSet.PatientPurchases)]
    public async Task<IActionResult> ListAll(
        [FromQuery] Guid? pacienteId = null,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _listAllPatientPurchasesService.ExecuteAsync(
            pacienteId,
            status,
            cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Paciente não encontrado."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<IReadOnlyList<PatientPurchaseDto>>(result.Value!, true));
    }

    [HttpGet("api/patients/{patientId:guid}/purchases")]
    [RequireAnyPermissionFrom(AuxiliaryPermissionSet.PatientPurchases)]
    public async Task<IActionResult> ListByPatient(
        Guid patientId,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _listPatientPurchasesService.ExecuteAsync(
            patientId,
            status,
            cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Paciente não encontrado."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<IReadOnlyList<PatientPurchaseDto>>(result.Value!, true));
    }

    [HttpGet("api/patients/{patientId:guid}/purchases/active")]
    [RequireAnyPermissionFrom(AuxiliaryPermissionSet.PatientPurchases)]
    public async Task<IActionResult> ListActiveByPatient(
        Guid patientId,
        CancellationToken cancellationToken = default)
    {
        var result = await _listActivePatientPurchasesService.ExecuteAsync(
            patientId,
            cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<IReadOnlyList<PatientPurchaseDto>>(result.Value!, true));
    }

    [HttpPost("api/patients/{patientId:guid}/purchases")]
    [RequirePermission("compra_paciente.criar")]
    public async Task<IActionResult> Create(
        Guid patientId,
        [FromBody] CreatePatientPurchaseRequest request,
        CancellationToken cancellationToken)
    {
        var payload = request with { PacienteId = patientId };
        var result = await _createPatientPurchasesService.ExecuteAsync(payload, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        return CreatedAtAction(
            nameof(Get),
            new { id = result.Value!.Id },
            new ApiResponse<PatientPurchaseDto>(result.Value, true));
    }

    [HttpGet("api/patient-purchases/{id:guid}")]
    [RequireAnyPermissionFrom(AuxiliaryPermissionSet.PatientPurchases)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getPatientPurchasesService.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<PatientPurchaseDto>(result.Value!, true));
    }

    [HttpGet("api/patient-purchases/{id:guid}/balance")]
    [RequireAnyPermissionFrom(AuxiliaryPermissionSet.PatientPurchases)]
    public async Task<IActionResult> GetBalance(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getPatientPurchaseBalanceService.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<PatientPurchaseBalanceDto>(result.Value!, true));
    }

    [HttpPost("api/patient-purchases/{id:guid}/cancel")]
    [RequirePermission("compra_paciente.cancelar")]
    public async Task<IActionResult> Cancel(
        Guid id,
        [FromBody] CancelPatientPurchaseRequest? request,
        CancellationToken cancellationToken)
    {
        var result = await _cancelPatientPurchasesService.ExecuteAsync(
            id,
            request ?? new CancelPatientPurchaseRequest(),
            cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Compra de pacote não encontrada."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<PatientPurchaseDto>(result.Value!, true));
    }
}
