using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Applications.Abstractions;
using BGD.CLINICAL.Application.Applications.Dtos;
using BGD.CLINICAL.Application.Common;

namespace BGD.CLINICAL.Application.Applications.PatientApplications;

public interface IGetPatientApplicationsService
{
    Task<Result<PatientApplicationDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

public sealed class GetPatientApplicationsService : IGetPatientApplicationsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IPatientApplicationsRepository _patientApplicationsRepository;

    public GetPatientApplicationsService(
        ICurrentTenantContext tenantContext,
        IPatientApplicationsRepository patientApplicationsRepository)
    {
        _tenantContext = tenantContext;
        _patientApplicationsRepository = patientApplicationsRepository;
    }

    public async Task<Result<PatientApplicationDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var aplicacao = await _patientApplicationsRepository.GetByIdAndEmpresaIdWithDetailsAsync(
            id,
            _tenantContext.EmpresaId,
            cancellationToken);

        if (aplicacao is null)
        {
            return Result<PatientApplicationDto>.Failure("Aplicação não encontrada.");
        }

        return Result<PatientApplicationDto>.Success(PatientApplicationsMapper.Map(aplicacao));
    }
}
