using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Application.Inventory.Dtos;
using BGD.CLINICAL.Domain.Enums;

namespace BGD.CLINICAL.Application.Inventory.SupplierOrders;

public interface IListSupplierOrdersService
{
    Task<Result<IReadOnlyList<SupplierOrderDto>>> ExecuteAsync(
        string? status,
        Guid? fornecedorId,
        Guid? unidadeId,
        CancellationToken cancellationToken = default);
}

public sealed class ListSupplierOrdersService : IListSupplierOrdersService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly ISupplierOrdersRepository _supplierOrdersRepository;

    public ListSupplierOrdersService(
        ICurrentTenantContext tenantContext,
        ISupplierOrdersRepository supplierOrdersRepository)
    {
        _tenantContext = tenantContext;
        _supplierOrdersRepository = supplierOrdersRepository;
    }

    public async Task<Result<IReadOnlyList<SupplierOrderDto>>> ExecuteAsync(
        string? status,
        Guid? fornecedorId,
        Guid? unidadeId,
        CancellationToken cancellationToken = default)
    {
        var statusResult = SupplierOrderValidation.ParseListStatus(status);
        if (statusResult.IsFailure)
        {
            return Result<IReadOnlyList<SupplierOrderDto>>.Failure(statusResult.Error!);
        }

        var pedidos = await _supplierOrdersRepository.ListByEmpresaIdAsync(
            _tenantContext.EmpresaId,
            statusResult.Value,
            fornecedorId,
            unidadeId,
            cancellationToken);

        return Result<IReadOnlyList<SupplierOrderDto>>.Success(SupplierOrdersMapper.Map(pedidos));
    }
}
