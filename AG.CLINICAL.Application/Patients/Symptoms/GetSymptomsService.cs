using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Patients.Abstractions;
using AG.CLINICAL.Application.Patients.Dtos;

namespace AG.CLINICAL.Application.Patients.Symptoms;

public interface IGetSymptomsService
{
    Task<Result<SymptomDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

public sealed class GetSymptomsService : IGetSymptomsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly ISymptomsRepository _symptomsRepository;

    public GetSymptomsService(
        ICurrentTenantContext tenantContext,
        ISymptomsRepository symptomsRepository)
    {
        _tenantContext = tenantContext;
        _symptomsRepository = symptomsRepository;
    }

    public async Task<Result<SymptomDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var sintoma = await _symptomsRepository.GetByIdAndEmpresaIdAsync(
            id,
            _tenantContext.EmpresaId,
            cancellationToken);

        if (sintoma is null)
        {
            return Result<SymptomDto>.Failure("Sintoma não encontrado.");
        }

        return Result<SymptomDto>.Success(SymptomsMapper.Map(sintoma));
    }
}
