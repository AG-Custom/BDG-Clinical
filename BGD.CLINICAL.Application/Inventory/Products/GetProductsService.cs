using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Application.Inventory.Dtos;

namespace BGD.CLINICAL.Application.Inventory.Products;

public interface IGetProductsService
{
    Task<Result<ProductDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

public sealed class GetProductsService : IGetProductsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IProductsRepository _productsRepository;

    public GetProductsService(
        ICurrentTenantContext tenantContext,
        IProductsRepository productsRepository)
    {
        _tenantContext = tenantContext;
        _productsRepository = productsRepository;
    }

    public async Task<Result<ProductDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var produto = await _productsRepository.GetByIdAndEmpresaIdAsync(
            id,
            _tenantContext.EmpresaId,
            cancellationToken);

        if (produto is null)
        {
            return Result<ProductDto>.Failure("Produto não encontrado.");
        }

        return Result<ProductDto>.Success(ProductsMapper.Map(produto));
    }
}
