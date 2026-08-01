using AG.CLINICAL.Domain.Entities;

namespace AG.CLINICAL.Application.Notifications.Abstractions;

public interface IEmailOutboxRepository
{
    Task AddAsync(OutputMessageEmail message, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OutputMessageEmail>> GetPendingBatchAsync(
        int batchSize,
        CancellationToken cancellationToken = default);

    void Update(OutputMessageEmail message);
}
