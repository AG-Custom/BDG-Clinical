using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.Application.Abstractions.Storage;
using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Inventory.Abstractions;
using AG.CLINICAL.Application.Inventory.Dtos;
using AG.CLINICAL.Domain.Enums;

namespace AG.CLINICAL.Application.Inventory.SupplierOrders;

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
    private readonly IObjectStorageService _objectStorageService;

    public ListSupplierOrdersService(
        ICurrentTenantContext tenantContext,
        ISupplierOrdersRepository supplierOrdersRepository,
        IObjectStorageService objectStorageService)
    {
        _tenantContext = tenantContext;
        _supplierOrdersRepository = supplierOrdersRepository;
        _objectStorageService = objectStorageService;
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

        return Result<IReadOnlyList<SupplierOrderDto>>.Success(
            SupplierOrdersMapper.Map(pedidos, _objectStorageService));
    }
}
