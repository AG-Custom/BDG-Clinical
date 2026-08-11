using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Inventory.Dtos;

namespace AG.CLINICAL.Application.Inventory.Abstractions;

public interface ICreateStockTransferService
{
    Task<Result<StockTransferDto>> ExecuteAsync(
        CreateStockTransferRequest request,
        CancellationToken cancellationToken = default);
}
