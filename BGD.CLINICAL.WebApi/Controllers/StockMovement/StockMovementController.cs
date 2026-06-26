using BGD.CLINICAL.Application.Inventory.Dtos;
using BGD.CLINICAL.Application.Inventory.StockMovements;
using BGD.CLINICAL.WebApi.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BGD.CLINICAL.WebApi.Controllers.StockMovement;

[ApiController]
[Authorize]
[Route("api/stock-movements")]
public sealed class StockMovementController : ControllerBase
{
    private readonly IListStockMovementsService _listStockMovementsService;
    private readonly ICreateStockAdjustmentsService _createStockAdjustmentsService;
    private readonly ICreateStockLossesService _createStockLossesService;

    public StockMovementController(
        IListStockMovementsService listStockMovementsService,
        ICreateStockAdjustmentsService createStockAdjustmentsService,
        ICreateStockLossesService createStockLossesService)
    {
        _listStockMovementsService = listStockMovementsService;
        _createStockAdjustmentsService = createStockAdjustmentsService;
        _createStockLossesService = createStockLossesService;
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] Guid? unidadeId = null,
        [FromQuery] Guid? produtoId = null,
        [FromQuery] string? tipo = null,
        [FromQuery] DateTime? dataInicio = null,
        [FromQuery] DateTime? dataFim = null,
        [FromQuery] int? limit = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _listStockMovementsService.ExecuteAsync(
            unidadeId,
            produtoId,
            tipo,
            dataInicio,
            dataFim,
            limit,
            cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<IReadOnlyList<StockMovementDto>>(result.Value!, true));
    }

    [HttpPost("adjustment")]
    public async Task<IActionResult> CreateAdjustment(
        [FromBody] CreateManualStockMovementRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createStockAdjustmentsService.ExecuteAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<StockMovementDto>(result.Value!, true));
    }

    [HttpPost("loss")]
    public async Task<IActionResult> CreateLoss(
        [FromBody] CreateManualStockMovementRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createStockLossesService.ExecuteAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<StockMovementDto>(result.Value!, true));
    }
}
