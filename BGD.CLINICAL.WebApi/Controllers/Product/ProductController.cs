using BGD.CLINICAL.Application.Inventory.Dtos;
using BGD.CLINICAL.Application.Inventory.Products;
using BGD.CLINICAL.WebApi.Authorization;
using BGD.CLINICAL.WebApi.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BGD.CLINICAL.WebApi.Controllers.Product;

[ApiController]
[Authorize]
[Route("api/products")]
public sealed class ProductController : ControllerBase
{
    private readonly ICreateProductsService _createProductsService;
    private readonly IListProductsService _listProductsService;
    private readonly IGetProductsService _getProductsService;
    private readonly IUpdateProductsService _updateProductsService;
    private readonly IDeactivateProductsService _deactivateProductsService;
    private readonly IReactivateProductsService _reactivateProductsService;

    public ProductController(
        ICreateProductsService createProductsService,
        IListProductsService listProductsService,
        IGetProductsService getProductsService,
        IUpdateProductsService updateProductsService,
        IDeactivateProductsService deactivateProductsService,
        IReactivateProductsService reactivateProductsService)
    {
        _createProductsService = createProductsService;
        _listProductsService = listProductsService;
        _getProductsService = getProductsService;
        _updateProductsService = updateProductsService;
        _deactivateProductsService = deactivateProductsService;
        _reactivateProductsService = reactivateProductsService;
    }

    [HttpGet]
    [RequireAnyPermissionFrom(AuxiliaryPermissionSet.Products)]
    public async Task<IActionResult> List(
        [FromQuery] Guid? tipoProdutoId = null,
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var result = await _listProductsService.ExecuteAsync(tipoProdutoId, includeInactive, cancellationToken);

        return Ok(new ApiResponse<IReadOnlyList<ProductDto>>(result.Value!, true));
    }

    [HttpGet("{id:guid}")]
    [RequireAnyPermissionFrom(AuxiliaryPermissionSet.Products)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getProductsService.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<ProductDto>(result.Value!, true));
    }

    [HttpPost]
    [RequirePermission("produto.criar")]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createProductsService.ExecuteAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        return CreatedAtAction(
            nameof(Get),
            new { id = result.Value!.Id },
            new ApiResponse<ProductDto>(result.Value, true));
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("produto.editar")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _updateProductsService.ExecuteAsync(id, request, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Produto não encontrado."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<ProductDto>(result.Value!, true));
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("produto.excluir")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _deactivateProductsService.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Produto não encontrado."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<ProductDto>(result.Value!, true));
    }

    [HttpPatch("{id:guid}/reactivate")]
    [RequirePermission("produto.editar")]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _reactivateProductsService.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Produto não encontrado."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<ProductDto>(result.Value!, true));
    }
}
