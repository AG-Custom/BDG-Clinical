using AG.CLINICAL.Application.Schedules.AppointmentTags;
using AG.CLINICAL.Application.Schedules.Dtos;
using AG.CLINICAL.WebApi.Authorization;
using AG.CLINICAL.WebApi.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AG.CLINICAL.WebApi.Controllers.AppointmentTag;

[ApiController]
[Authorize]
[Route("api/appointment-tags")]
public sealed class AppointmentTagController : ControllerBase
{
    private readonly ICreateAppointmentTagsService _createAppointmentTagsService;
    private readonly IListAppointmentTagsService _listAppointmentTagsService;
    private readonly IGetAppointmentTagsService _getAppointmentTagsService;
    private readonly IUpdateAppointmentTagsService _updateAppointmentTagsService;
    private readonly IDeactivateAppointmentTagsService _deactivateAppointmentTagsService;
    private readonly IReactivateAppointmentTagsService _reactivateAppointmentTagsService;

    public AppointmentTagController(
        ICreateAppointmentTagsService createAppointmentTagsService,
        IListAppointmentTagsService listAppointmentTagsService,
        IGetAppointmentTagsService getAppointmentTagsService,
        IUpdateAppointmentTagsService updateAppointmentTagsService,
        IDeactivateAppointmentTagsService deactivateAppointmentTagsService,
        IReactivateAppointmentTagsService reactivateAppointmentTagsService)
    {
        _createAppointmentTagsService = createAppointmentTagsService;
        _listAppointmentTagsService = listAppointmentTagsService;
        _getAppointmentTagsService = getAppointmentTagsService;
        _updateAppointmentTagsService = updateAppointmentTagsService;
        _deactivateAppointmentTagsService = deactivateAppointmentTagsService;
        _reactivateAppointmentTagsService = reactivateAppointmentTagsService;
    }

    [HttpGet]
    [RequireAnyPermissionFrom(AuxiliaryPermissionSet.AppointmentTags)]
    public async Task<IActionResult> List(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var result = await _listAppointmentTagsService.ExecuteAsync(includeInactive, cancellationToken);

        return Ok(new ApiResponse<IReadOnlyList<AppointmentTagCatalogDto>>(result.Value!, true));
    }

    [HttpGet("{id:guid}")]
    [RequireAnyPermissionFrom(AuxiliaryPermissionSet.AppointmentTags)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getAppointmentTagsService.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<AppointmentTagCatalogDto>(result.Value!, true));
    }

    [HttpPost]
    [RequirePermission("tag_agendamento.criar")]
    public async Task<IActionResult> Create(
        [FromBody] CreateAppointmentTagRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createAppointmentTagsService.ExecuteAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        return CreatedAtAction(
            nameof(Get),
            new { id = result.Value!.Id },
            new ApiResponse<AppointmentTagCatalogDto>(result.Value, true));
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("tag_agendamento.editar")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateAppointmentTagRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _updateAppointmentTagsService.ExecuteAsync(id, request, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Tag não encontrada."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<AppointmentTagCatalogDto>(result.Value!, true));
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("tag_agendamento.excluir")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _deactivateAppointmentTagsService.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Tag não encontrada."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<AppointmentTagCatalogDto>(result.Value!, true));
    }

    [HttpPatch("{id:guid}/reactivate")]
    [RequirePermission("tag_agendamento.editar")]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _reactivateAppointmentTagsService.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Tag não encontrada."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<AppointmentTagCatalogDto>(result.Value!, true));
    }
}
