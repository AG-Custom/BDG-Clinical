using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Application.Patients.Abstractions;
using BGD.CLINICAL.Application.Patients.Dtos;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using BGD.CLINICAL.Domain.Exceptions;

namespace BGD.CLINICAL.Application.Patients.Patients;

public interface ICreatePatientsService
{
    Task<Result<PatientDto>> ExecuteAsync(
        CreatePatientRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class CreatePatientsService : ICreatePatientsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IPatientsRepository _patientsRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePatientsService(
        ICurrentTenantContext tenantContext,
        IPatientsRepository patientsRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _patientsRepository = patientsRepository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PatientDto>> ExecuteAsync(
        CreatePatientRequest request,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;

        var validation = await PatientRequestValidator.ValidateAsync(
            empresaId,
            request.UnidadeId,
            request.Nome,
            request.Cpf,
            request.Telefone,
            request.Email,
            request.Observacao,
            excludePatientId: null,
            _patientsRepository,
            cancellationToken);

        if (validation.IsFailure)
        {
            return Result<PatientDto>.Failure(validation.Error!);
        }

        try
        {
            var data = validation.Value!;
            var paciente = Paciente.Create(
                empresaId,
                data.UnidadeId,
                data.Nome,
                data.Cpf,
                data.Telefone,
                data.Email,
                request.DataNascimento,
                data.Observacao);

            await _patientsRepository.AddAsync(paciente, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditLogsService.RegisterEntityChangeAsync(
                empresaId,
                _tenantContext.UsuarioId,
                nameof(Paciente),
                paciente.Id,
                AcaoAuditoria.Criar,
                dadosNovos: PatientsAuditSerializer.Serialize(paciente),
                cancellationToken: cancellationToken);

            return Result<PatientDto>.Success(PatientsMapper.Map(paciente));
        }
        catch (DomainException exception)
        {
            return Result<PatientDto>.Failure(exception.Message);
        }
    }
}
