using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Patients.Abstractions;
using BGD.CLINICAL.Application.Patients.Dtos;

namespace BGD.CLINICAL.Application.Patients.Patients;

public interface IGetPatientsService
{
    Task<Result<PatientDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

public sealed class GetPatientsService : IGetPatientsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IPatientsRepository _patientsRepository;

    public GetPatientsService(
        ICurrentTenantContext tenantContext,
        IPatientsRepository patientsRepository)
    {
        _tenantContext = tenantContext;
        _patientsRepository = patientsRepository;
    }

    public async Task<Result<PatientDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var paciente = await _patientsRepository.GetByIdAndEmpresaIdAsync(
            id,
            _tenantContext.EmpresaId,
            cancellationToken);

        if (paciente is null)
        {
            return Result<PatientDto>.Failure("Paciente não encontrado.");
        }

        return Result<PatientDto>.Success(PatientsMapper.Map(paciente));
    }
}
