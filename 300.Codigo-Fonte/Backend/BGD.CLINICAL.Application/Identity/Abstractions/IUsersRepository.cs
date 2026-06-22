using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Application.Identity.Abstractions;

public interface IUsersRepository
{
    Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Usuario>> ListByEmailLoginAsync(string emailLogin, CancellationToken cancellationToken = default);

    Task<bool> ExistsActiveEmailLoginAsync(string emailLogin, CancellationToken cancellationToken = default);

    Task<bool> ExistsActiveByEmailAsync(string emailLogin, CancellationToken cancellationToken = default);

    Task AddAsync(Usuario usuario, CancellationToken cancellationToken = default);
}
