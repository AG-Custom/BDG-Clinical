using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Application.Inventory.Dtos;

namespace BGD.CLINICAL.Application.Inventory.Products;

public interface IListProductsService
{
    Task<Result<IReadOnlyList<ProductDto>>> ExecuteAsync(
        Guid? tipoProdutoId = null,
        bool includeInactive = false,
        CancellationToken cancellationToken = default);
}

public sealed class ListProductsService : IListProductsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IProductsRepository _productsRepository;

    public ListProductsService(
        ICurrentTenantContext tenantContext,
        IProductsRepository productsRepository)
    {
        _tenantContext = tenantContext;
        _productsRepository = productsRepository;
    }

    public async Task<Result<IReadOnlyList<ProductDto>>> ExecuteAsync(
        Guid? tipoProdutoId = null,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var produtos = await _productsRepository.ListByEmpresaIdAsync(
            _tenantContext.EmpresaId,
            tipoProdutoId,
            includeInactive,
            cancellationToken);

        return Result<IReadOnlyList<ProductDto>>.Success(ProductsMapper.Map(produtos));
    }
}
