using BGD.CLINICAL.Application.Inventory.Dtos;
using BGD.CLINICAL.Application.Inventory.StockBalances;
using BGD.CLINICAL.WebApi.Authorization;
using BGD.CLINICAL.WebApi.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BGD.CLINICAL.WebApi.Controllers.StockBalance;

[ApiController]
[Authorize]
[Route("api/stock-balances")]
public sealed class StockBalanceController : ControllerBase
{
    private readonly IListStockBalancesService _listStockBalancesService;
    private readonly IListStockLotBalancesService _listStockLotBalancesService;

    public StockBalanceController(
        IListStockBalancesService listStockBalancesService,
        IListStockLotBalancesService listStockLotBalancesService)
    {
        _listStockBalancesService = listStockBalancesService;
        _listStockLotBalancesService = listStockLotBalancesService;
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
