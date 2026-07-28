namespace BGD.CLINICAL.Application.Audits.Abstractions;

public sealed record EntityAuditUserIds(
    Guid? IdUsuarioCriacao,
    Guid? IdUsuarioAtualizacao);

public interface IAuditLogsQueryRepository
{
    Task<EntityAuditUserIds> GetUserIdsForEntityAsync(
        Guid empresaId,
        string entidade,
        Guid registroId,
        CancellationToken cancellationToken = default);
}
