using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Applications.Abstractions;
using BGD.CLINICAL.Application.Applications.Dtos;
using BGD.CLINICAL.Application.Common;

namespace BGD.CLINICAL.Application.Applications.PatientApplications;

public interface IListPatientApplicationsService
{
    Task<Result<IReadOnlyList<PatientApplicationDto>>> ExecuteAsync(
        Guid? pacienteId = null,
        Guid? unidadeId = null,
        Guid? produtoId = null,
        Guid? aplicadorId = null,
        bool? cancelada = null,
        DateTime? dataInicio = null,
        DateTime? dataFim = null,
        int? limit = null,
        CancellationToken cancellationToken = default);
}

public sealed class ListPatientApplicationsService : IListPatientApplicationsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IPatientApplicationsRepository _patientApplicationsRepository;

    public ListPatientApplicationsService(
        ICurrentTenantContext tenantContext,
        IPatientApplicationsRepository patientApplicationsRepository)
    {
        _tenantContext = tenantContext;
        _patientApplicationsRepository = patientApplicationsRepository;
    }

    public async Task<Result<IReadOnlyList<PatientApplicationDto>>> ExecuteAsync(
        Guid? pacienteId = null,
        Guid? unidadeId = null,
        Guid? produtoId = null,
        Guid? aplicadorId = null,
        bool? cancelada = null,
        DateTime? dataInicio = null,
        DateTime? dataFim = null,
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        var limitResult = PatientApplicationRequestValidator.ValidateListLimit(limit);
        if (limitResult.IsFailure)
        {
            return Result<IReadOnlyList<PatientApplicationDto>>.Failure(limitResult.Error!);
        }

        var aplicacoes = await _patientApplicationsRepository.ListByEmpresaIdAsync(
            _tenantContext.EmpresaId,
            pacienteId,
            unidadeId,
            produtoId,
            aplicadorId,
            cancelada,
            dataInicio,
            dataFim,
            limitResult.Value,
            cancellationToken);

        return Result<IReadOnlyList<PatientApplicationDto>>.Success(
            PatientApplicationsMapper.Map(aplicacoes));
    }
}
