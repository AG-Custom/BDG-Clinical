using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Applications.Abstractions;
using BGD.CLINICAL.Application.Patients.Abstractions;
using BGD.CLINICAL.Application.Applications.Dtos;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using BGD.CLINICAL.Domain.Exceptions;

namespace BGD.CLINICAL.Application.Applications.PatientApplications;

public interface IUpdatePatientApplicationsService
{
    Task<Result<PatientApplicationDto>> ExecuteAsync(
        Guid id,
        UpdatePatientApplicationRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class UpdatePatientApplicationsService : IUpdatePatientApplicationsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IPatientApplicationsRepository _patientApplicationsRepository;
    private readonly ISymptomsRepository _symptomsRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePatientApplicationsService(
        ICurrentTenantContext tenantContext,
        IPatientApplicationsRepository patientApplicationsRepository,
        ISymptomsRepository symptomsRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _patientApplicationsRepository = patientApplicationsRepository;
        _symptomsRepository = symptomsRepository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PatientApplicationDto>> ExecuteAsync(
        Guid id,
        UpdatePatientApplicationRequest request,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;

        var aplicacao = await _patientApplicationsRepository.GetByIdAndEmpresaIdWithDetailsAsync(
            id,
            empresaId,
            cancellationToken);

        if (aplicacao is null)
        {
            return Result<PatientApplicationDto>.Failure("Aplicação não encontrada.");
        }

        var validation = await PatientApplicationRequestValidator.ValidateUpdateAsync(
            empresaId,
            request,
            _symptomsRepository,
            cancellationToken);

        if (validation.IsFailure)
        {
            return Result<PatientApplicationDto>.Failure(validation.Error!);
        }

        try
        {
            var dadosAnteriores = PatientApplicationsAuditSerializer.Serialize(aplicacao);

            aplicacao.UpdateDetails(request.Peso, request.Observacao, request.DataAplicacao);
            _patientApplicationsRepository.Update(aplicacao);
            await _patientApplicationsRepository.ReplaceSymptomsAsync(
                aplicacao.Id,
                validation.Value!,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var persisted = await _patientApplicationsRepository.GetByIdAndEmpresaIdWithDetailsAsync(
                aplicacao.Id,
                empresaId,
                cancellationToken);

            await _auditLogsService.RegisterEntityChangeAsync(
                empresaId,
                _tenantContext.UsuarioId,
                nameof(AplicacaoPaciente),
                aplicacao.Id,
                AcaoAuditoria.Editar,
                dadosAnteriores: dadosAnteriores,
                dadosNovos: PatientApplicationsAuditSerializer.Serialize(persisted ?? aplicacao),
                cancellationToken: cancellationToken);

            return Result<PatientApplicationDto>.Success(
                PatientApplicationsMapper.Map(persisted ?? aplicacao));
        }
        catch (DomainException exception)
        {
            return Result<PatientApplicationDto>.Failure(exception.Message);
        }
    }
}
