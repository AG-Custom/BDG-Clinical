using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Inventory.Abstractions;
using AG.CLINICAL.Application.Inventory.Dtos;

namespace AG.CLINICAL.Application.Inventory.ProductTypes;

public interface IListProductTypesService
{
    Task<Result<IReadOnlyList<ProductTypeDto>>> ExecuteAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default);
}

public sealed class ListProductTypesService : IListProductTypesService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IProductTypesRepository _productTypesRepository;

    public ListProductTypesService(
        ICurrentTenantContext tenantContext,
        IProductTypesRepository productTypesRepository)
    {
        _tenantContext = tenantContext;
        _productTypesRepository = productTypesRepository;
    }

    public async Task<Result<IReadOnlyList<ProductTypeDto>>> ExecuteAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var tipos = await _productTypesRepository.ListByEmpresaIdAsync(
            _tenantContext.EmpresaId,
            includeInactive,
            cancellationToken);

        return Result<IReadOnlyList<ProductTypeDto>>.Success(ProductTypesMapper.Map(tipos));
    }
}
