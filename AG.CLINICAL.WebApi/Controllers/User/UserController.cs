using AG.CLINICAL.Application.Identity.Dtos;
using AG.CLINICAL.Application.Identity.Users;
using AG.CLINICAL.WebApi.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AG.CLINICAL.WebApi.Controllers.User;

[ApiController]
[Authorize]
[Route("api/users")]
public sealed class UserController : ControllerBase
{
    private readonly IResolveUserDisplayNamesService _resolveUserDisplayNamesService;

    public UserController(IResolveUserDisplayNamesService resolveUserDisplayNamesService)
    {
        _resolveUserDisplayNamesService = resolveUserDisplayNamesService;
    }

    [HttpPost("display-names")]
    public async Task<IActionResult> DisplayNames(
        [FromBody] ResolveUserDisplayNamesRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _resolveUserDisplayNamesService.ExecuteAsync(
            request?.Ids ?? [],
            cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<IReadOnlyList<UserDisplayNameDto>>(result.Value!, true));
    }
}
