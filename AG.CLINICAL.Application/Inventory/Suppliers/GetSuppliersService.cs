using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Inventory.Abstractions;
using AG.CLINICAL.Application.Inventory.Dtos;

namespace AG.CLINICAL.Application.Inventory.Suppliers;

public interface IGetSuppliersService
{
    Task<Result<SupplierDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

public sealed class GetSuppliersService : IGetSuppliersService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly ISuppliersRepository _suppliersRepository;

    public GetSuppliersService(
        ICurrentTenantContext tenantContext,
        ISuppliersRepository suppliersRepository)
    {
        _tenantContext = tenantContext;
        _suppliersRepository = suppliersRepository;
    }

    public async Task<Result<SupplierDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var fornecedor = await _suppliersRepository.GetByIdAndEmpresaIdAsync(
            id,
            _tenantContext.EmpresaId,
            cancellationToken);

        if (fornecedor is null)
        {
            return Result<SupplierDto>.Failure("Fornecedor não encontrado.");
        }

        return Result<SupplierDto>.Success(SuppliersMapper.Map(fornecedor));
    }
}
