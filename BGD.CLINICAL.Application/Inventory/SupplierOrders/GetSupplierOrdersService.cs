using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Application.Inventory.Dtos;

namespace BGD.CLINICAL.Application.Inventory.SupplierOrders;

public interface IGetSupplierOrdersService
{
    Task<Result<SupplierOrderDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

public sealed class GetSupplierOrdersService : IGetSupplierOrdersService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly ISupplierOrdersRepository _supplierOrdersRepository;

    public GetSupplierOrdersService(
        ICurrentTenantContext tenantContext,
        ISupplierOrdersRepository supplierOrdersRepository)
    {
        _tenantContext = tenantContext;
        _supplierOrdersRepository = supplierOrdersRepository;
    }

    public async Task<Result<SupplierOrderDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var pedido = await _supplierOrdersRepository.GetByIdAndEmpresaIdWithItensAsync(
            id,
            _tenantContext.EmpresaId,
            cancellationToken);

        if (pedido is null)
        {
            return Result<SupplierOrderDto>.Failure("Pedido não encontrado.");
        }

        return Result<SupplierOrderDto>.Success(SupplierOrdersMapper.Map(pedido));
    }
}
