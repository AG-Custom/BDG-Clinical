using BGD.CLINICAL.Application.Inventory.Dtos;
using BGD.CLINICAL.Application.Inventory.StockBalances;
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

    public StockBalanceController(IListStockBalancesService listStockBalancesService)
    {
        _listStockBalancesService = listStockBalancesService;
    }

    [HttpGet]
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
}
