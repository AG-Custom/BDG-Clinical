using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.Application.Applications.Abstractions;
using AG.CLINICAL.Application.Applications.Dtos;
using AG.CLINICAL.Application.Common;

namespace AG.CLINICAL.Application.Applications.PatientApplications;

public interface IListPatientApplicationsService
{
    Task<Result<IReadOnlyList<PatientApplicationDto>>> ExecuteAsync(
        Guid? pacienteId = null,
        Guid? unidadeId = null,
        Guid? produtoId = null,
        Guid? procedimentoId = null,
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
        Guid? procedimentoId = null,
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
            procedimentoId,
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
