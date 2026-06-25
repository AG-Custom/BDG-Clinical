using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Patients.Abstractions;
using BGD.CLINICAL.Application.Patients.Dtos;

namespace BGD.CLINICAL.Application.Patients.Patients;

public interface IListPatientsService
{
    Task<Result<IReadOnlyList<PatientDto>>> ExecuteAsync(
        Guid? unidadeId = null,
        bool includeInactive = false,
        CancellationToken cancellationToken = default);
}

public sealed class ListPatientsService : IListPatientsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IPatientsRepository _patientsRepository;

    public ListPatientsService(
        ICurrentTenantContext tenantContext,
        IPatientsRepository patientsRepository)
    {
        _tenantContext = tenantContext;
        _patientsRepository = patientsRepository;
    }

    public async Task<Result<IReadOnlyList<PatientDto>>> ExecuteAsync(
        Guid? unidadeId = null,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var pacientes = await _patientsRepository.ListByEmpresaIdAsync(
            _tenantContext.EmpresaId,
            unidadeId,
            includeInactive,
            cancellationToken);

        return Result<IReadOnlyList<PatientDto>>.Success(PatientsMapper.Map(pacientes));
    }
}
