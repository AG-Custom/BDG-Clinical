using AG.CLINICAL.Application.Abstractions.Persistence;
using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.Application.Applications.Abstractions;
using AG.CLINICAL.Application.Applications.Dtos;
using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Identity.Abstractions;
using AG.CLINICAL.Domain.Entities;
using AG.CLINICAL.Domain.Enums;

namespace AG.CLINICAL.Application.Applications.Procedures;

public interface IDeactivateProceduresService
{
    Task<Result<ProcedureDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

public sealed class DeactivateProceduresService : IDeactivateProceduresService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IProceduresRepository _proceduresRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateProceduresService(
        ICurrentTenantContext tenantContext,
        IProceduresRepository proceduresRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _proceduresRepository = proceduresRepository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProcedureDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;
        var procedimento = await _proceduresRepository.GetByIdAndEmpresaIdWithDetailsAsync(
            id,
            empresaId,
            cancellationToken);

        if (procedimento is null)
        {
            return Result<ProcedureDto>.Failure("Procedimento não encontrado.");
        }

        if (!procedimento.Ativo)
        {
            return Result<ProcedureDto>.Failure("Procedimento já está inativo.");
        }

        var dadosAnteriores = ProceduresAuditSerializer.Serialize(procedimento);

        procedimento.Deactivate();
        _proceduresRepository.Update(procedimento);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogsService.RegisterEntityChangeAsync(
            empresaId,
            _tenantContext.UsuarioId,
            nameof(Procedimento),
            procedimento.Id,
            AcaoAuditoria.Excluir,
            dadosAnteriores: dadosAnteriores,
            dadosNovos: ProceduresAuditSerializer.Serialize(procedimento),
            cancellationToken: cancellationToken);

        return Result<ProcedureDto>.Success(ProceduresMapper.Map(procedimento));
    }
}
