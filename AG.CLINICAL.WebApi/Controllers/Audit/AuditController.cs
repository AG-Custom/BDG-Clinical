using AG.CLINICAL.Application.Audits;
using AG.CLINICAL.Application.Audits.Dtos;
using AG.CLINICAL.WebApi.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AG.CLINICAL.WebApi.Controllers.Audit;

[ApiController]
[Authorize]
[Route("api/audit")]
public sealed class AuditController : ControllerBase
{
    private readonly IGetEntityAuditSummaryService _getEntityAuditSummaryService;

    public AuditController(IGetEntityAuditSummaryService getEntityAuditSummaryService)
    {
        _getEntityAuditSummaryService = getEntityAuditSummaryService;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> Summary(
        [FromQuery] string entidade,
        [FromQuery] Guid registroId,
        CancellationToken cancellationToken)
    {
        var result = await _getEntityAuditSummaryService.ExecuteAsync(
            entidade,
            registroId,
            cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<EntityAuditSummaryDto>(result.Value!, true));
    }
}
