using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using BGD.CLINICAL.Infra.Data.Context;

namespace BGD.CLINICAL.Infra.Data.Services.Audits;

public sealed class AuditLogsService : IAuditLogsService
{
    private readonly AppDbContext _context;

    public AuditLogsService(AppDbContext context)
    {
        _context = context;
    }

    public async Task RegisterLoginAsync(
        Guid usuarioId,
        Guid empresaId,
        string? ip,
        CancellationToken cancellationToken = default)
    {
        var log = new LogAuditoria(
            empresaId,
            usuarioId,
            nameof(Usuario),
            usuarioId,
            AcaoAuditoria.Login,
            DateTime.UtcNow,
            ip);

        await _context.LogsAuditoria.AddAsync(log, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RegisterEntityChangeAsync(
        Guid empresaId,
        Guid usuarioId,
        string entidade,
        Guid registroId,
        AcaoAuditoria acao,
        string? dadosAnteriores = null,
        string? dadosNovos = null,
        CancellationToken cancellationToken = default)
    {
        var log = new LogAuditoria(
            empresaId,
            usuarioId,
            entidade,
            registroId,
            acao,
            DateTime.UtcNow,
            ip: null,
            dadosAnteriores,
            dadosNovos);

        await _context.LogsAuditoria.AddAsync(log, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
