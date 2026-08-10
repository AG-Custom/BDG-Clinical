using AG.CLINICAL.Application.Schedules.Dtos;
using AG.CLINICAL.Application.Schedules.UnitOperatingHours;
using AG.CLINICAL.WebApi.Authorization;
using AG.CLINICAL.WebApi.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AG.CLINICAL.WebApi.Controllers.UnitOperatingHour;

[ApiController]
[Authorize]
[Route("api/unit-operating-hours/{id:guid}/active")]
public sealed class UnitOperatingHourActiveController : ControllerBase
{
    private readonly ISetUnitOperatingHourActiveStatusService _setActiveStatusService;

    public UnitOperatingHourActiveController(ISetUnitOperatingHourActiveStatusService setActiveStatusService)
    {
        _setActiveStatusService = setActiveStatusService;
    }

    [HttpPatch]
    [RequirePermission("unidade.editar")]
    public async Task<IActionResult> SetActive(
        Guid id,
        [FromBody] SetUnitOperatingHourActiveRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _setActiveStatusService.ExecuteAsync(id, request, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Horário de funcionamento não encontrado."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<UnitOperatingHourDto>(result.Value!, true));
    }
}
