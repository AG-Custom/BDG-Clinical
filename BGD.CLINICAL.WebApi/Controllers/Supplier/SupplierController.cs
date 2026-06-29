using BGD.CLINICAL.Application.Inventory.Dtos;
using BGD.CLINICAL.Application.Inventory.Suppliers;
using BGD.CLINICAL.WebApi.Authorization;
using BGD.CLINICAL.WebApi.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BGD.CLINICAL.WebApi.Controllers.Supplier;

[ApiController]
[Authorize]
[Route("api/suppliers")]
public sealed class SupplierController : ControllerBase
{
    private readonly ICreateSuppliersService _createSuppliersService;
    private readonly IListSuppliersService _listSuppliersService;
    private readonly IGetSuppliersService _getSuppliersService;
    private readonly IUpdateSuppliersService _updateSuppliersService;
    private readonly IDeactivateSuppliersService _deactivateSuppliersService;
    private readonly IReactivateSuppliersService _reactivateSuppliersService;

    public SupplierController(
        ICreateSuppliersService createSuppliersService,
        IListSuppliersService listSuppliersService,
        IGetSuppliersService getSuppliersService,
        IUpdateSuppliersService updateSuppliersService,
        IDeactivateSuppliersService deactivateSuppliersService,
        IReactivateSuppliersService reactivateSuppliersService)
    {
        _createSuppliersService = createSuppliersService;
        _listSuppliersService = listSuppliersService;
        _getSuppliersService = getSuppliersService;
        _updateSuppliersService = updateSuppliersService;
        _deactivateSuppliersService = deactivateSuppliersService;
        _reactivateSuppliersService = reactivateSuppliersService;
    }

    [HttpGet]
    [RequireAnyPermissionFrom(AuxiliaryPermissionSet.Suppliers)]
    public async Task<IActionResult> List(
        [FromQuery] bool includeInactive = false,
        [FromQuery] string? search = null,
        [FromQuery] int? limit = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _listSuppliersService.ExecuteAsync(includeInactive, search, limit, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<IReadOnlyList<SupplierDto>>(result.Value!, true));
    }

    [HttpGet("{id:guid}")]
    [RequireAnyPermissionFrom(AuxiliaryPermissionSet.Suppliers)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getSuppliersService.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<SupplierDto>(result.Value!, true));
    }

    [HttpPost]
    [RequirePermission("fornecedor.criar")]
    public async Task<IActionResult> Create(
        [FromBody] CreateSupplierRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createSuppliersService.ExecuteAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        return CreatedAtAction(
            nameof(Get),
            new { id = result.Value!.Id },
            new ApiResponse<SupplierDto>(result.Value, true));
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("fornecedor.editar")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateSupplierRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _updateSuppliersService.ExecuteAsync(id, request, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Fornecedor não encontrado."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<SupplierDto>(result.Value!, true));
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("fornecedor.excluir")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _deactivateSuppliersService.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Fornecedor não encontrado."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<SupplierDto>(result.Value!, true));
    }

    [HttpPatch("{id:guid}/reactivate")]
    [RequirePermission("fornecedor.editar")]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _reactivateSuppliersService.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Fornecedor não encontrado."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<SupplierDto>(result.Value!, true));
    }
}
