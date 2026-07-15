using BGD.CLINICAL.Application.Packages.Dtos;
using BGD.CLINICAL.Application.Packages.Packages;
using BGD.CLINICAL.WebApi.Authorization;
using BGD.CLINICAL.WebApi.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BGD.CLINICAL.WebApi.Controllers.Package;

[ApiController]
[Authorize]
[Route("api/packages")]
public sealed class PackageController : ControllerBase
{
    private readonly ICreatePackagesService _createPackagesService;
    private readonly IListPackagesService _listPackagesService;
    private readonly IGetPackagesService _getPackagesService;
    private readonly IUpdatePackagesService _updatePackagesService;
    private readonly IDeactivatePackagesService _deactivatePackagesService;
    private readonly IReactivatePackagesService _reactivatePackagesService;

    public PackageController(
        ICreatePackagesService createPackagesService,
        IListPackagesService listPackagesService,
        IGetPackagesService getPackagesService,
        IUpdatePackagesService updatePackagesService,
        IDeactivatePackagesService deactivatePackagesService,
        IReactivatePackagesService reactivatePackagesService)
    {
        _createPackagesService = createPackagesService;
        _listPackagesService = listPackagesService;
        _getPackagesService = getPackagesService;
        _updatePackagesService = updatePackagesService;
        _deactivatePackagesService = deactivatePackagesService;
        _reactivatePackagesService = reactivatePackagesService;
    }

    [HttpGet]
    [RequireAnyPermissionFrom(AuxiliaryPermissionSet.Packages)]
    public async Task<IActionResult> List(
        [FromQuery] bool includeInactive = false,
        [FromQuery] string? search = null,
        [FromQuery] int? limit = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _listPackagesService.ExecuteAsync(
            includeInactive,
            search,
            limit,
            cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<IReadOnlyList<PackageDto>>(result.Value!, true));
    }

    [HttpGet("{id:guid}")]
    [RequireAnyPermissionFrom(AuxiliaryPermissionSet.Packages)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getPackagesService.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<PackageDto>(result.Value!, true));
    }

    [HttpPost]
    [RequirePermission("pacote.criar")]
    public async Task<IActionResult> Create(
        [FromBody] CreatePackageRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createPackagesService.ExecuteAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        return CreatedAtAction(
            nameof(Get),
            new { id = result.Value!.Id },
            new ApiResponse<PackageDto>(result.Value, true));
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("pacote.editar")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdatePackageRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _updatePackagesService.ExecuteAsync(id, request, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Pacote não encontrado."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<PackageDto>(result.Value!, true));
    }

    [HttpPatch("{id:guid}/deactivate")]
    [RequirePermission("pacote.editar")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _deactivatePackagesService.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Pacote não encontrado."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<PackageDto>(result.Value!, true));
    }

    [HttpPatch("{id:guid}/reactivate")]
    [RequirePermission("pacote.editar")]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _reactivatePackagesService.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Pacote não encontrado."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<PackageDto>(result.Value!, true));
    }
}
