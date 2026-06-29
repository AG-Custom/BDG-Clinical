using BGD.CLINICAL.Application.Schedules.Appointments;
using BGD.CLINICAL.Application.Schedules.Dtos;
using BGD.CLINICAL.WebApi.Authorization;
using BGD.CLINICAL.WebApi.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BGD.CLINICAL.WebApi.Controllers.Appointment;

[ApiController]
[Authorize]
[Route("api/appointments")]
public sealed class AppointmentController : ControllerBase
{
    private readonly ICreateAppointmentsService _createAppointmentsService;
    private readonly IListAppointmentsService _listAppointmentsService;
    private readonly IGetAppointmentsService _getAppointmentsService;
    private readonly IUpdateAppointmentsService _updateAppointmentsService;
    private readonly IConfirmAppointmentsService _confirmAppointmentsService;
    private readonly ICompleteAppointmentsService _completeAppointmentsService;
    private readonly ICancelAppointmentsService _cancelAppointmentsService;
    private readonly IMarkNoShowAppointmentsService _markNoShowAppointmentsService;

    public AppointmentController(
        ICreateAppointmentsService createAppointmentsService,
        IListAppointmentsService listAppointmentsService,
        IGetAppointmentsService getAppointmentsService,
        IUpdateAppointmentsService updateAppointmentsService,
        IConfirmAppointmentsService confirmAppointmentsService,
        ICompleteAppointmentsService completeAppointmentsService,
        ICancelAppointmentsService cancelAppointmentsService,
        IMarkNoShowAppointmentsService markNoShowAppointmentsService)
    {
        _createAppointmentsService = createAppointmentsService;
        _listAppointmentsService = listAppointmentsService;
        _getAppointmentsService = getAppointmentsService;
        _updateAppointmentsService = updateAppointmentsService;
        _confirmAppointmentsService = confirmAppointmentsService;
        _completeAppointmentsService = completeAppointmentsService;
        _cancelAppointmentsService = cancelAppointmentsService;
        _markNoShowAppointmentsService = markNoShowAppointmentsService;
    }

    [HttpGet]
    [RequirePermission("agenda.visualizar")]
    public async Task<IActionResult> List(
        [FromQuery] Guid? unidadeId = null,
        [FromQuery] Guid? funcionarioId = null,
        [FromQuery] Guid? pacienteId = null,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? dataInicioFrom = null,
        [FromQuery] DateTime? dataInicioTo = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _listAppointmentsService.ExecuteAsync(
            unidadeId,
            funcionarioId,
            pacienteId,
            status,
            dataInicioFrom,
            dataInicioTo,
            cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<IReadOnlyList<AppointmentDto>>(result.Value!, true));
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("agenda.visualizar")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getAppointmentsService.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<AppointmentDto>(result.Value!, true));
    }

    [HttpPost]
    [RequirePermission("agendamento.criar")]
    public async Task<IActionResult> Create(
        [FromBody] CreateAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createAppointmentsService.ExecuteAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        return CreatedAtAction(
            nameof(Get),
            new { id = result.Value!.Id },
            new ApiResponse<AppointmentDto>(result.Value, true));
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("agendamento.editar")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _updateAppointmentsService.ExecuteAsync(id, request, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Agendamento não encontrado."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<AppointmentDto>(result.Value!, true));
    }

    [HttpPatch("{id:guid}/confirm")]
    [RequirePermission("agendamento.confirmar")]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken cancellationToken)
    {
        var result = await _confirmAppointmentsService.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Agendamento não encontrado."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<AppointmentDto>(result.Value!, true));
    }

    [HttpPatch("{id:guid}/complete")]
    [RequirePermission("agendamento.concluir")]
    public async Task<IActionResult> Complete(
        Guid id,
        [FromBody] CompleteAppointmentRequest? request,
        CancellationToken cancellationToken)
    {
        var result = await _completeAppointmentsService.ExecuteAsync(
            id,
            request ?? new CompleteAppointmentRequest(),
            cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Agendamento não encontrado."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<AppointmentDto>(result.Value!, true));
    }

    [HttpPatch("{id:guid}/cancel")]
    [RequirePermission("agendamento.cancelar")]
    public async Task<IActionResult> Cancel(
        Guid id,
        [FromBody] CancelAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _cancelAppointmentsService.ExecuteAsync(id, request, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Agendamento não encontrado."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<AppointmentDto>(result.Value!, true));
    }

    [HttpPatch("{id:guid}/no-show")]
    [RequirePermission("agendamento.registrar_falta")]
    public async Task<IActionResult> MarkNoShow(Guid id, CancellationToken cancellationToken)
    {
        var result = await _markNoShowAppointmentsService.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Agendamento não encontrado."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<AppointmentDto>(result.Value!, true));
    }
}
