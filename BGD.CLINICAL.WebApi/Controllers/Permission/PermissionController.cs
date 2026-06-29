using BGD.CLINICAL.Application.Modules.Dtos;
using BGD.CLINICAL.Application.Modules.Permissions;
using BGD.CLINICAL.WebApi.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BGD.CLINICAL.WebApi.Controllers.Permission;

[ApiController]
[Authorize]
[Route("api/permissions")]
public sealed class PermissionController : ControllerBase
{
    private readonly IGetPermissionMapService _getPermissionMapService;

    public PermissionController(IGetPermissionMapService getPermissionMapService)
    {
        _getPermissionMapService = getPermissionMapService;
    }

    [HttpGet("map")]
    public async Task<IActionResult> GetMap(CancellationToken cancellationToken)
    {
        var result = await _getPermissionMapService.ExecuteAsync(cancellationToken);
        return Ok(new ApiResponse<IReadOnlyList<PermissionMapNodeDto>>(result.Value!, true));
    }
}
