using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Application.Patients.Abstractions;
using BGD.CLINICAL.Application.Patients.Dtos;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;

namespace BGD.CLINICAL.Application.Patients.Patients;

public interface IReactivatePatientsService
{
    Task<Result<PatientDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

public sealed class ReactivatePatientsService : IReactivatePatientsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IPatientsRepository _patientsRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public ReactivatePatientsService(
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
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;
        var paciente = await _patientsRepository.GetByIdAndEmpresaIdAsync(id, empresaId, cancellationToken);

        if (paciente is null)
        {
            return Result<PatientDto>.Failure("Paciente não encontrado.");
        }

        if (paciente.Ativo)
        {
            return Result<PatientDto>.Failure("Paciente já está ativo.");
        }

        if (!await _patientsRepository.ExistsActiveUnidadeInEmpresaAsync(paciente.UnidadeId, empresaId, cancellationToken))
        {
            return Result<PatientDto>.Failure("A unidade vinculada ao paciente está inativa.");
        }

        if (paciente.Cpf is not null
            && await _patientsRepository.ExistsByCpfAsync(empresaId, paciente.Cpf, paciente.Id, cancellationToken))
        {
            return Result<PatientDto>.Failure("Já existe um paciente ativo com este CPF nesta empresa.");
        }

        var dadosAnteriores = PatientsAuditSerializer.Serialize(paciente);

        paciente.Reactivate();
        _patientsRepository.Update(paciente);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogsService.RegisterEntityChangeAsync(
            empresaId,
            _tenantContext.UsuarioId,
            nameof(Paciente),
            paciente.Id,
            AcaoAuditoria.Editar,
            dadosAnteriores: dadosAnteriores,
            dadosNovos: PatientsAuditSerializer.Serialize(paciente),
            cancellationToken: cancellationToken);

        return Result<PatientDto>.Success(PatientsMapper.Map(paciente));
    }
}
