using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Audits.Abstractions;
using BGD.CLINICAL.Application.Audits.Dtos;
using BGD.CLINICAL.Application.Common;

namespace BGD.CLINICAL.Application.Audits;

public interface IGetEntityAuditSummaryService
{
    Task<Result<EntityAuditSummaryDto>> ExecuteAsync(
        string entidade,
        Guid registroId,
        CancellationToken cancellationToken = default);
}

public sealed class GetEntityAuditSummaryService : IGetEntityAuditSummaryService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IAuditLogsQueryRepository _auditLogsQueryRepository;

    public GetEntityAuditSummaryService(
        ICurrentTenantContext tenantContext,
        IAuditLogsQueryRepository auditLogsQueryRepository)
    {
        _tenantContext = tenantContext;
        _auditLogsQueryRepository = auditLogsQueryRepository;
    }

    public async Task<Result<EntityAuditSummaryDto>> ExecuteAsync(
        string entidade,
        Guid registroId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(entidade))
        {
            return Result<EntityAuditSummaryDto>.Failure("Entidade é obrigatória.");
        }

        if (registroId == Guid.Empty)
        {
            return Result<EntityAuditSummaryDto>.Failure("Registro inválido.");
        }

        var userIds = await _auditLogsQueryRepository.GetUserIdsForEntityAsync(
            _tenantContext.EmpresaId,
            entidade.Trim(),
            registroId,
            cancellationToken);

        return Result<EntityAuditSummaryDto>.Success(
            new EntityAuditSummaryDto(userIds.IdUsuarioCriacao, userIds.IdUsuarioAtualizacao));
    }
}
