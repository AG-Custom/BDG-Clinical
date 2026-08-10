namespace AG.CLINICAL.Application.Audits.Abstractions;

public sealed record EntityAuditUserIds(
    Guid? IdUsuarioCriacao,
    Guid? IdUsuarioAtualizacao);

public sealed record EntityAuditLogRecord(
    Guid Id,
    Guid UsuarioId,
    AcaoAuditoria Acao,
    DateTime Data,
    string? DadosAnteriores,
    string? DadosNovos);

public interface IAuditLogsQueryRepository
{
    Task<EntityAuditUserIds> GetUserIdsForEntityAsync(
        Guid empresaId,
        string entidade,
        Guid registroId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EntityAuditLogRecord>> ListByEntityAsync(
        Guid empresaId,
        string entidade,
        Guid registroId,
        IReadOnlyList<AcaoAuditoria>? acoes = null,
        CancellationToken cancellationToken = default);
}
