using BGD.CLINICAL.Application.Inventory.Dtos;
using BGD.CLINICAL.Application.Inventory.SupplierOrders;
using BGD.CLINICAL.WebApi.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BGD.CLINICAL.WebApi.Controllers.SupplierOrder;

[ApiController]
[Authorize]
[Route("api/supplier-orders")]
public sealed class SupplierOrderController : ControllerBase
{
    private readonly ICreateSupplierOrdersService _createSupplierOrdersService;
    private readonly IListSupplierOrdersService _listSupplierOrdersService;
    private readonly IGetSupplierOrdersService _getSupplierOrdersService;
    private readonly IUpdateSupplierOrdersService _updateSupplierOrdersService;
    private readonly ICancelSupplierOrdersService _cancelSupplierOrdersService;
    private readonly IReceiveSupplierOrdersService _receiveSupplierOrdersService;

    public SupplierOrderController(
        ICreateSupplierOrdersService createSupplierOrdersService,
        IListSupplierOrdersService listSupplierOrdersService,
        IGetSupplierOrdersService getSupplierOrdersService,
        IUpdateSupplierOrdersService updateSupplierOrdersService,
        ICancelSupplierOrdersService cancelSupplierOrdersService,
        IReceiveSupplierOrdersService receiveSupplierOrdersService)
    {
        _createSupplierOrdersService = createSupplierOrdersService;
        _listSupplierOrdersService = listSupplierOrdersService;
        _getSupplierOrdersService = getSupplierOrdersService;
        _updateSupplierOrdersService = updateSupplierOrdersService;
        _cancelSupplierOrdersService = cancelSupplierOrdersService;
        _receiveSupplierOrdersService = receiveSupplierOrdersService;
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? status = null,
        [FromQuery] Guid? fornecedorId = null,
        [FromQuery] Guid? unidadeId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _listSupplierOrdersService.ExecuteAsync(status, fornecedorId, unidadeId, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<IReadOnlyList<SupplierOrderDto>>(result.Value!, true));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getSupplierOrdersService.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<SupplierOrderDto>(result.Value!, true));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateSupplierOrderRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createSupplierOrdersService.ExecuteAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        return CreatedAtAction(
            nameof(Get),
            new { id = result.Value!.Id },
            new ApiResponse<SupplierOrderDto>(result.Value, true));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateSupplierOrderRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _updateSupplierOrdersService.ExecuteAsync(id, request, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Pedido não encontrado."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<SupplierOrderDto>(result.Value!, true));
    }

    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var result = await _cancelSupplierOrdersService.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Pedido não encontrado."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<SupplierOrderDto>(result.Value!, true));
    }

    [HttpPatch("{id:guid}/receive")]
    public async Task<IActionResult> Receive(Guid id, CancellationToken cancellationToken)
    {
        var result = await _receiveSupplierOrdersService.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Pedido não encontrado."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<SupplierOrderDto>(result.Value!, true));
    }
}
