using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Patients.Abstractions;
using AG.CLINICAL.Application.Patients.Dtos;

namespace AG.CLINICAL.Application.Patients.Symptoms;

public interface IListSymptomsService
{
    Task<Result<IReadOnlyList<SymptomDto>>> ExecuteAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default);
}

public sealed class ListSymptomsService : IListSymptomsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly ISymptomsRepository _symptomsRepository;

    public ListSymptomsService(
        ICurrentTenantContext tenantContext,
        ISymptomsRepository symptomsRepository)
    {
        _tenantContext = tenantContext;
        _symptomsRepository = symptomsRepository;
    }

    public async Task<Result<IReadOnlyList<SymptomDto>>> ExecuteAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var sintomas = await _symptomsRepository.ListByEmpresaIdAsync(
            _tenantContext.EmpresaId,
            includeInactive,
            cancellationToken);

        return Result<IReadOnlyList<SymptomDto>>.Success(SymptomsMapper.Map(sintomas));
    }
}
