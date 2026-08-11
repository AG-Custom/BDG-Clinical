using AG.CLINICAL.Application.Reports;
using AG.CLINICAL.Application.Reports.Dtos;
using AG.CLINICAL.WebApi.Authorization;
using AG.CLINICAL.WebApi.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AG.CLINICAL.WebApi.Controllers.Report;

[ApiController, Authorize, Route("api/reports")]
public sealed class ReportController(IGetOperationalReportService service) : ControllerBase
{
    [HttpGet("operational"), RequirePermission("relatorio.visualizar")]
    public async Task<IActionResult> GetOperational([FromQuery] DateOnly? dataInicio = null, [FromQuery] DateOnly? dataFim = null, CancellationToken cancellationToken = default)
    {
        var result = await service.ExecuteAsync(dataInicio, dataFim, cancellationToken);
        if (result.IsFailure) return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        return Ok(new ApiResponse<OperationalReportDto>(result.Value!, true));
    }
}
