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

public interface IUpdatePatientsService
{
    Task<Result<PatientDto>> ExecuteAsync(
        Guid id,
        UpdatePatientRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class UpdatePatientsService : IUpdatePatientsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IPatientsRepository _patientsRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePatientsService(
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
        Guid id,
        UpdatePatientRequest request,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;
        var paciente = await _patientsRepository.GetByIdAndEmpresaIdWithDetailsAsync(id, empresaId, cancellationToken);

        if (paciente is null)
        {
            return Result<PatientDto>.Failure("Paciente não encontrado.");
        }

        if (!paciente.Ativo)
        {
            return Result<PatientDto>.Failure("Não é possível editar um paciente inativo.");
        }

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
            excludePatientId: id,
            _patientsRepository,
            cancellationToken);

        if (validation.IsFailure)
        {
            return Result<PatientDto>.Failure(validation.Error!);
        }

        try
        {
            var dadosAnteriores = PatientsAuditSerializer.Serialize(paciente);
            var data = validation.Value!;

            paciente.UpdateDetails(
                data.UnidadeIds,
                data.Nome,
                data.Cpf,
                data.Telefone,
                data.Email,
                request.DataNascimento,
                data.Endereco,
                data.Observacao,
                PatientSexoParser.Parse(request.Sexo));

            _patientsRepository.Update(paciente);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var persisted = await _patientsRepository.GetByIdAndEmpresaIdWithDetailsAsync(
                id,
                empresaId,
                cancellationToken);

            await _auditLogsService.RegisterEntityChangeAsync(
                empresaId,
                _tenantContext.UsuarioId,
                nameof(Paciente),
                paciente.Id,
                AcaoAuditoria.Editar,
                dadosAnteriores: dadosAnteriores,
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
