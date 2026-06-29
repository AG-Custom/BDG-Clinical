using BGD.CLINICAL.Application.Inventory.Dtos;
using BGD.CLINICAL.Application.Inventory.ProductTypes;
using BGD.CLINICAL.WebApi.Authorization;
using BGD.CLINICAL.WebApi.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BGD.CLINICAL.WebApi.Controllers.ProductType;

[ApiController]
[Authorize]
[Route("api/product-types")]
public sealed class ProductTypeController : ControllerBase
{
    private readonly ICreateProductTypesService _createProductTypesService;
    private readonly IListProductTypesService _listProductTypesService;
    private readonly IGetProductTypesService _getProductTypesService;
    private readonly IUpdateProductTypesService _updateProductTypesService;
    private readonly IDeactivateProductTypesService _deactivateProductTypesService;
    private readonly IReactivateProductTypesService _reactivateProductTypesService;

    public ProductTypeController(
        ICreateProductTypesService createProductTypesService,
        IListProductTypesService listProductTypesService,
        IGetProductTypesService getProductTypesService,
        IUpdateProductTypesService updateProductTypesService,
        IDeactivateProductTypesService deactivateProductTypesService,
        IReactivateProductTypesService reactivateProductTypesService)
    {
        _createProductTypesService = createProductTypesService;
        _listProductTypesService = listProductTypesService;
        _getProductTypesService = getProductTypesService;
        _updateProductTypesService = updateProductTypesService;
        _deactivateProductTypesService = deactivateProductTypesService;
        _reactivateProductTypesService = reactivateProductTypesService;
    }

    [HttpGet]
    [RequireAnyPermissionFrom(AuxiliaryPermissionSet.ProductTypes)]
    public async Task<IActionResult> List(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var result = await _listProductTypesService.ExecuteAsync(includeInactive, cancellationToken);

        return Ok(new ApiResponse<IReadOnlyList<ProductTypeDto>>(result.Value!, true));
    }

    [HttpGet("{id:guid}")]
    [RequireAnyPermissionFrom(AuxiliaryPermissionSet.ProductTypes)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getProductTypesService.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<ProductTypeDto>(result.Value!, true));
    }

    [HttpPost]
    [RequirePermission("tipo_produto.criar")]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductTypeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createProductTypesService.ExecuteAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        return CreatedAtAction(
            nameof(Get),
            new { id = result.Value!.Id },
            new ApiResponse<ProductTypeDto>(result.Value, true));
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("tipo_produto.editar")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateProductTypeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _updateProductTypesService.ExecuteAsync(id, request, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Tipo de produto não encontrado."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<ProductTypeDto>(result.Value!, true));
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("tipo_produto.excluir")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _deactivateProductTypesService.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Tipo de produto não encontrado."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<ProductTypeDto>(result.Value!, true));
    }

    [HttpPatch("{id:guid}/reactivate")]
    [RequirePermission("tipo_produto.editar")]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _reactivateProductTypesService.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Tipo de produto não encontrado."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<ProductTypeDto>(result.Value!, true));
    }
}
