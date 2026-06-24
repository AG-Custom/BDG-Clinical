using BGD.CLINICAL.Domain.Enums;

namespace BGD.CLINICAL.Application.Identity.Abstractions;

public interface IAuditLogsService
{
    Task RegisterLoginAsync(
        Guid usuarioId,
        Guid empresaId,
        string? ip,
        CancellationToken cancellationToken = default);

    Task RegisterEntityChangeAsync(
        Guid empresaId,
        Guid usuarioId,
        string entidade,
        Guid registroId,
        AcaoAuditoria acao,
        string? dadosAnteriores = null,
        string? dadosNovos = null,
        CancellationToken cancellationToken = default);
}
