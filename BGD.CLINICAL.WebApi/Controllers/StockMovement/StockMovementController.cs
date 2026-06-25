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

    public StockMovementController(IListStockMovementsService listStockMovementsService)
    {
        _listStockMovementsService = listStockMovementsService;
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
}
