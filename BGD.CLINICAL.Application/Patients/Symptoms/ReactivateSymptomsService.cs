using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Application.Patients.Abstractions;
using BGD.CLINICAL.Application.Patients.Dtos;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;

namespace BGD.CLINICAL.Application.Patients.Symptoms;

public interface IReactivateSymptomsService
{
    Task<Result<SymptomDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

public sealed class ReactivateSymptomsService : IReactivateSymptomsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly ISymptomsRepository _symptomsRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public ReactivateSymptomsService(
        ICurrentTenantContext tenantContext,
        ISymptomsRepository symptomsRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _symptomsRepository = symptomsRepository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SymptomDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;
        var sintoma = await _symptomsRepository.GetByIdAndEmpresaIdAsync(id, empresaId, cancellationToken);

        if (sintoma is null)
        {
            return Result<SymptomDto>.Failure("Sintoma não encontrado.");
        }

        if (sintoma.Ativo)
        {
            return Result<SymptomDto>.Failure("Sintoma já está ativo.");
        }

        if (await _symptomsRepository.ExistsByNomeAsync(empresaId, sintoma.Nome, sintoma.Id, cancellationToken))
        {
            return Result<SymptomDto>.Failure("Já existe um sintoma com este nome.");
        }

        var dadosAnteriores = SymptomsAuditSerializer.Serialize(sintoma);

        sintoma.Reactivate();
        _symptomsRepository.Update(sintoma);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogsService.RegisterEntityChangeAsync(
            empresaId,
            _tenantContext.UsuarioId,
            nameof(Sintoma),
            sintoma.Id,
            AcaoAuditoria.Editar,
            dadosAnteriores: dadosAnteriores,
            dadosNovos: SymptomsAuditSerializer.Serialize(sintoma),
            cancellationToken: cancellationToken);

        return Result<SymptomDto>.Success(SymptomsMapper.Map(sintoma));
    }
}
