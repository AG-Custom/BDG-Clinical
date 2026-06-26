using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Applications.Abstractions;
using BGD.CLINICAL.Application.Applications.Dtos;
using BGD.CLINICAL.Application.Common;

namespace BGD.CLINICAL.Application.Applications.Procedures;

public interface IListProceduresService
{
    Task<Result<IReadOnlyList<ProcedureDto>>> ExecuteAsync(
        bool includeInactive,
        string? search,
        Guid? produtoAplicadoId,
        int? limit,
        CancellationToken cancellationToken = default);
}

public sealed class ListProceduresService : IListProceduresService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IProceduresRepository _proceduresRepository;

    public ListProceduresService(
        ICurrentTenantContext tenantContext,
        IProceduresRepository proceduresRepository)
    {
        _tenantContext = tenantContext;
        _proceduresRepository = proceduresRepository;
    }

    public async Task<Result<IReadOnlyList<ProcedureDto>>> ExecuteAsync(
        bool includeInactive,
        string? search,
        Guid? produtoAplicadoId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        if (produtoAplicadoId.HasValue && produtoAplicadoId.Value == Guid.Empty)
        {
            return Result<IReadOnlyList<ProcedureDto>>.Failure("Produto aplicado inválido.");
        }

        string? normalizedSearch = null;
        int? effectiveLimit = null;

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchResult = ProcedureRequestValidator.ValidateSearch(search, limit);
            if (searchResult.IsFailure)
            {
                return Result<IReadOnlyList<ProcedureDto>>.Failure(searchResult.Error!);
            }

            (normalizedSearch, var validatedLimit) = searchResult.Value!;
            effectiveLimit = validatedLimit;
        }
        else if (limit.HasValue)
        {
            var limitResult = ProcedureRequestValidator.ValidateLimit(
                limit,
                ProcedureSearchOptions.DefaultLimit);

            if (limitResult.IsFailure)
            {
                return Result<IReadOnlyList<ProcedureDto>>.Failure(limitResult.Error!);
            }

            effectiveLimit = limitResult.Value;
        }

        var procedimentos = await _proceduresRepository.ListByEmpresaIdAsync(
            _tenantContext.EmpresaId,
            includeInactive,
            produtoAplicadoId,
            normalizedSearch,
            effectiveLimit,
            cancellationToken);

        return Result<IReadOnlyList<ProcedureDto>>.Success(ProceduresMapper.Map(procedimentos));
    }
}
