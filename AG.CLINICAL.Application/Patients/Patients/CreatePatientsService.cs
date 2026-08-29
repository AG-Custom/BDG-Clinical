using AG.CLINICAL.Application.Abstractions.Persistence;
using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Identity.Abstractions;
using AG.CLINICAL.Application.Patients.Abstractions;
using AG.CLINICAL.Application.Patients.Dtos;
using AG.CLINICAL.Domain.Entities;
using AG.CLINICAL.Domain.Enums;
using AG.CLINICAL.Domain.Exceptions;

namespace AG.CLINICAL.Application.Patients.Patients;

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
            request.UnidadeIds,
            request.Nome,
            request.Cpf,
            request.Telefone,
            request.Email,
            request.Endereco,
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
                data.UnidadeIds,
                data.Nome,
                data.Cpf,
                data.Telefone,
                data.Email,
                request.DataNascimento,
                data.Endereco,
                data.Observacao,
                PatientSexoParser.Parse(request.Sexo));

            await _patientsRepository.AddAsync(paciente, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var persisted = await _patientsRepository.GetByIdAndEmpresaIdWithDetailsAsync(
                paciente.Id,
                empresaId,
                cancellationToken);

            await _auditLogsService.RegisterEntityChangeAsync(
                empresaId,
                _tenantContext.UsuarioId,
                nameof(Paciente),
                paciente.Id,
                AcaoAuditoria.Criar,
                dadosNovos: PatientsAuditSerializer.Serialize(persisted ?? paciente),
                cancellationToken: cancellationToken);

            return Result<PatientDto>.Success(PatientsMapper.Map(persisted ?? paciente));
        }
        catch (DomainException exception)
        {
            return Result<PatientDto>.Failure(exception.Message);
        }
    }
}
