using AG.CLINICAL.Domain.Entities;

namespace AG.CLINICAL.Application.Identity.Abstractions;

public sealed record UsuarioDisplayName(Guid Id, string Nome);

public interface IUsersRepository
{
    Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UsuarioDisplayName>> ListDisplayNamesByIdsAndEmpresaIdAsync(
        Guid empresaId,
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Usuario>> ListByEmailLoginAsync(string emailLogin, CancellationToken cancellationToken = default);

    Task<Usuario?> GetByEmailLoginAndEmpresaIdAsync(
        string emailLogin,
        Guid empresaId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsActiveEmailLoginAsync(string emailLogin, CancellationToken cancellationToken = default);

    Task<bool> ExistsActiveByEmailAsync(string emailLogin, CancellationToken cancellationToken = default);

    Task<bool> ExistsActiveEmailLoginByEmpresaAsync(
        Guid empresaId,
        string emailLogin,
        CancellationToken cancellationToken = default);

    Task<Usuario?> GetByFuncionarioIdAndEmpresaIdAsync(
        Guid funcionarioId,
        Guid empresaId,
        CancellationToken cancellationToken = default);

    Task AddAsync(Usuario usuario, CancellationToken cancellationToken = default);

    void Update(Usuario usuario);
}
