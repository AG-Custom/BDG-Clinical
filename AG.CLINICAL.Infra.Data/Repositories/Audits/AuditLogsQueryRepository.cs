using AG.CLINICAL.Application.Audits.Abstractions;
using AG.CLINICAL.Domain.Enums;
using AG.CLINICAL.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace AG.CLINICAL.Infra.Data.Repositories.Audits;

public sealed class AuditLogsQueryRepository : IAuditLogsQueryRepository
{
    private static readonly AcaoAuditoria[] AcoesMutacao =
    [
        AcaoAuditoria.Editar,
        AcaoAuditoria.Excluir,
        AcaoAuditoria.Cancelar,
        AcaoAuditoria.GerarMovimentacao,
        AcaoAuditoria.SincronizarGoogle,
    ];

    private readonly AppDbContext _context;

    public AuditLogsQueryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<EntityAuditUserIds> GetUserIdsForEntityAsync(
        Guid empresaId,
        string entidade,
        Guid registroId,
        CancellationToken cancellationToken = default)
    {
        var logs = await _context.LogsAuditoria
            .AsNoTracking()
            .Where(log =>
                log.EmpresaId == empresaId
                && log.Entidade == entidade
                && log.RegistroId == registroId)
            .Select(log => new { log.UsuarioId, log.Acao, log.Data })
            .ToListAsync(cancellationToken);

        if (logs.Count == 0)
        {
            return new EntityAuditUserIds(null, null);
        }

        var criacao = logs
            .Where(log => log.Acao == AcaoAuditoria.Criar)
            .OrderBy(log => log.Data)
            .Select(log => (Guid?)log.UsuarioId)
            .FirstOrDefault();

        if (criacao is null)
        {
            criacao = logs
                .Where(log => AcoesMutacao.Contains(log.Acao))
                .OrderBy(log => log.Data)
                .Select(log => (Guid?)log.UsuarioId)
                .FirstOrDefault();
        }

        var atualizacao = logs
            .Where(log => AcoesMutacao.Contains(log.Acao))
            .OrderByDescending(log => log.Data)
            .Select(log => (Guid?)log.UsuarioId)
            .FirstOrDefault();

        return new EntityAuditUserIds(criacao, atualizacao);
    }
}
