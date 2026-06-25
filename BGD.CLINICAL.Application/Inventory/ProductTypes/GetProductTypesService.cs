using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Application.Inventory.Dtos;

namespace BGD.CLINICAL.Application.Inventory.ProductTypes;

public interface IGetProductTypesService
{
    Task<Result<ProductTypeDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

public sealed class GetProductTypesService : IGetProductTypesService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IProductTypesRepository _productTypesRepository;

    public GetProductTypesService(
        ICurrentTenantContext tenantContext,
        IProductTypesRepository productTypesRepository)
    {
        _tenantContext = tenantContext;
        _productTypesRepository = productTypesRepository;
    }

    public async Task<Result<ProductTypeDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var tipoProduto = await _productTypesRepository.GetByIdAndEmpresaIdAsync(
            id,
            _tenantContext.EmpresaId,
            cancellationToken);

        if (tipoProduto is null)
        {
            return Result<ProductTypeDto>.Failure("Tipo de produto não encontrado.");
        }

        return Result<ProductTypeDto>.Success(ProductTypesMapper.Map(tipoProduto));
    }
}
