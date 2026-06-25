using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Application.Inventory.Dtos;

namespace BGD.CLINICAL.Application.Inventory.ProductTypes;

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
