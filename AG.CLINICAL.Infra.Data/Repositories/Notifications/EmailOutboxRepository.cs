using AG.CLINICAL.Application.Notifications.Abstractions;
using AG.CLINICAL.Domain.Entities;
using AG.CLINICAL.Domain.Enums;
using AG.CLINICAL.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace AG.CLINICAL.Infra.Data.Repositories.Notifications;

public sealed class EmailOutboxRepository : IEmailOutboxRepository
{
    private readonly AppDbContext _context;

    public EmailOutboxRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(OutputMessageEmail message, CancellationToken cancellationToken = default)
    {
        await _context.OutputMessagesEmail.AddAsync(message, cancellationToken);
    }

    public async Task<IReadOnlyList<OutputMessageEmail>> GetPendingBatchAsync(
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        return await _context.OutputMessagesEmail
            .Where(message =>
                message.Status == EmailOutboxStatus.Pendente
                && message.Tentativas < OutputMessageEmail.MaxRetryAttempts)
            .OrderBy(message => message.CriadoEm)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public void Update(OutputMessageEmail message)
    {
        if (_context.Entry(message).State == EntityState.Detached)
        {
            _context.OutputMessagesEmail.Update(message);
        }
    }
}
