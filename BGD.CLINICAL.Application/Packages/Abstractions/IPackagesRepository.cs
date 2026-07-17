using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Application.Packages.Abstractions;

public interface IPackagesRepository
{
    Task<IReadOnlyList<Pacote>> ListByEmpresaIdAsync(
        Guid empresaId,
        bool includeInactive,
        string? search,
        int? limit,
        CancellationToken cancellationToken = default);

    Task<Pacote?> GetByIdAndEmpresaIdWithDetailsAsync(
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

    Task AddAsync(Pacote pacote, CancellationToken cancellationToken = default);

    void Update(Pacote pacote);
}
