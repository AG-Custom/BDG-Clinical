using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Application.Patients.Abstractions;
using BGD.CLINICAL.Application.Patients.Dtos;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;

namespace BGD.CLINICAL.Application.Patients.Patients;

public interface IDeactivatePatientsService
{
    Task<Result<PatientDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

public sealed class DeactivatePatientsService : IDeactivatePatientsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IPatientsRepository _patientsRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivatePatientsService(
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

        if (!paciente.Ativo)
        {
            return Result<PatientDto>.Failure("Paciente já está inativo.");
        }

        var dadosAnteriores = PatientsAuditSerializer.Serialize(paciente);

        paciente.Deactivate();
        _patientsRepository.Update(paciente);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogsService.RegisterEntityChangeAsync(
            empresaId,
            _tenantContext.UsuarioId,
            nameof(Paciente),
            paciente.Id,
            AcaoAuditoria.Excluir,
            dadosAnteriores: dadosAnteriores,
            dadosNovos: PatientsAuditSerializer.Serialize(paciente),
            cancellationToken: cancellationToken);

        return Result<PatientDto>.Success(PatientsMapper.Map(paciente));
    }
}
