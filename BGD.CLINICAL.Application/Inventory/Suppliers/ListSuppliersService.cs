using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Application.Inventory.Dtos;

namespace BGD.CLINICAL.Application.Inventory.Suppliers;

public interface IListSuppliersService
{
    Task<Result<IReadOnlyList<SupplierDto>>> ExecuteAsync(
        bool includeInactive,
        string? search,
        int? limit,
        CancellationToken cancellationToken = default);
}

public sealed class ListSuppliersService : IListSuppliersService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly ISuppliersRepository _suppliersRepository;

    public ListSuppliersService(
        ICurrentTenantContext tenantContext,
        ISuppliersRepository suppliersRepository)
    {
        _tenantContext = tenantContext;
        _suppliersRepository = suppliersRepository;
    }

    public async Task<Result<IReadOnlyList<SupplierDto>>> ExecuteAsync(
        bool includeInactive,
        string? search,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        string? normalizedSearch = null;
        int? effectiveLimit = null;

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchResult = SupplierValidation.ValidateSearch(search, limit);
            if (searchResult.IsFailure)
            {
                return Result<IReadOnlyList<SupplierDto>>.Failure(searchResult.Error!);
            }

            (normalizedSearch, var validatedLimit) = searchResult.Value!;
            effectiveLimit = validatedLimit;
        }
        else if (limit.HasValue)
        {
            var limitResult = SupplierValidation.ValidateLimit(limit, SupplierSearchOptions.DefaultLimit);
            if (limitResult.IsFailure)
            {
                return Result<IReadOnlyList<SupplierDto>>.Failure(limitResult.Error!);
            }

            effectiveLimit = limitResult.Value;
        }

        var fornecedores = await _suppliersRepository.ListByEmpresaIdAsync(
            _tenantContext.EmpresaId,
            includeInactive,
            normalizedSearch,
            effectiveLimit,
            cancellationToken);

        return Result<IReadOnlyList<SupplierDto>>.Success(SuppliersMapper.Map(fornecedores));
    }
}
