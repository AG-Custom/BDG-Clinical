using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Application.Core.Abstractions;

public interface IPositionsRepository
{
    Task<IReadOnlyList<Cargo>> ListByEmpresaIdAsync(
        Guid empresaId,
        bool includeInactive,
        CancellationToken cancellationToken = default);

    Task<Cargo?> GetByIdAndEmpresaIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByNomeAsync(
        Guid empresaId,
        string nome,
        Guid? excludeId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsActiveByIdAndEmpresaIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default);

    Task AddAsync(Cargo cargo, CancellationToken cancellationToken = default);

    void Update(Cargo cargo);
}
