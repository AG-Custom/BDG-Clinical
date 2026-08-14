using AG.CLINICAL.Application.Inventory.Abstractions;
using AG.CLINICAL.Application.Inventory.Dtos;
using AG.CLINICAL.Application.Inventory.StockMovements;
using AG.CLINICAL.WebApi.Authorization;
using AG.CLINICAL.WebApi.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AG.CLINICAL.WebApi.Controllers.StockMovement;

[ApiController]
[Authorize]
[Route("api/stock-movements")]
public sealed class StockMovementController : ControllerBase
{
    private readonly IListStockMovementsService _listStockMovementsService;
    private readonly ICreateStockAdjustmentsService _createStockAdjustmentsService;
    private readonly ICreateStockLossesService _createStockLossesService;
    private readonly ICreateStockTransferService _createStockTransferService;

    public StockMovementController(
        IListStockMovementsService listStockMovementsService,
        ICreateStockAdjustmentsService createStockAdjustmentsService,
        ICreateStockLossesService createStockLossesService,
        ICreateStockTransferService createStockTransferService)
    {
        _listStockMovementsService = listStockMovementsService;
        _createStockAdjustmentsService = createStockAdjustmentsService;
        _createStockLossesService = createStockLossesService;
        _createStockTransferService = createStockTransferService;
    }

    [HttpGet]
    [RequirePermission("estoque.visualizar")]
    public async Task<IActionResult> List(
        [FromQuery] Guid? unidadeId = null,
        [FromQuery] Guid? produtoId = null,
        [FromQuery] string? tipo = null,
        [FromQuery] DateTime? dataInicio = null,
        [FromQuery] DateTime? dataFim = null,
        [FromQuery] int? limit = null,
        [FromQuery] Guid? transferenciaEstoqueId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _listStockMovementsService.ExecuteAsync(
            unidadeId,
            produtoId,
            tipo,
            dataInicio,
            dataFim,
            limit,
            transferenciaEstoqueId,
            cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<IReadOnlyList<StockMovementDto>>(result.Value!, true));
    }

    [HttpPost("adjustment")]
    [RequirePermission("estoque.ajustar")]
    public async Task<IActionResult> CreateAdjustment(
        [FromBody] CreateManualStockMovementRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createStockAdjustmentsService.ExecuteAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<IReadOnlyList<StockMovementDto>>(result.Value!, true));
    }

    [HttpPost("loss")]
    [RequirePermission("estoque.movimentar")]
    public async Task<IActionResult> CreateLoss(
        [FromBody] CreateManualStockMovementRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createStockLossesService.ExecuteAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<IReadOnlyList<StockMovementDto>>(result.Value!, true));
    }

    [HttpPost("transfer")]
    [RequirePermission("estoque.movimentar")]
    public async Task<IActionResult> CreateTransfer(
        [FromBody] CreateStockTransferRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createStockTransferService.ExecuteAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<StockTransferDto>(result.Value!, true));
    }
}
