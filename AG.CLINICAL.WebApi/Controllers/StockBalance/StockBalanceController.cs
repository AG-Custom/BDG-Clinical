using AG.CLINICAL.Application.Inventory.Dtos;
using AG.CLINICAL.Application.Inventory.StockBalances;
using AG.CLINICAL.WebApi.Authorization;
using AG.CLINICAL.WebApi.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AG.CLINICAL.WebApi.Controllers.StockBalance;

[ApiController]
[Authorize]
[Route("api/stock-balances")]
public sealed class StockBalanceController : ControllerBase
{
    private readonly IListStockBalancesService _listStockBalancesService;
    private readonly IListStockLotBalancesService _listStockLotBalancesService;
    private readonly IUpdateStockBalanceService _updateStockBalanceService;

    public StockBalanceController(
        IListStockBalancesService listStockBalancesService,
        IListStockLotBalancesService listStockLotBalancesService,
        IUpdateStockBalanceService updateStockBalanceService)
    {
        _listStockBalancesService = listStockBalancesService;
        _listStockLotBalancesService = listStockLotBalancesService;
        _updateStockBalanceService = updateStockBalanceService;
    }

    [HttpPut]
    [RequirePermission("estoque.ajustar")]
    public async Task<IActionResult> Update(
        [FromBody] UpdateStockBalanceRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _updateStockBalanceService.ExecuteAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<IReadOnlyList<StockMovementDto>>(result.Value!, true));
    }

    [HttpGet]
    [RequirePermission("estoque.visualizar")]
    public async Task<IActionResult> List(
        [FromQuery] Guid? unidadeId = null,
        [FromQuery] Guid? produtoId = null,
        [FromQuery] bool? abaixoDoMinimo = null,
        [FromQuery] string? search = null,
        [FromQuery] int? limit = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _listStockBalancesService.ExecuteAsync(
            unidadeId,
            produtoId,
            abaixoDoMinimo,
            search,
            limit,
            cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<IReadOnlyList<StockBalanceDto>>(result.Value!, true));
    }

    [HttpGet("lots")]
    [RequirePermission("estoque.visualizar")]
    public async Task<IActionResult> ListLots(
        [FromQuery] Guid? unidadeId = null,
        [FromQuery] Guid? produtoId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _listStockLotBalancesService.ExecuteAsync(
            unidadeId,
            produtoId,
            cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<IReadOnlyList<StockLotBalanceDto>>(result.Value!, true));
    }
}
