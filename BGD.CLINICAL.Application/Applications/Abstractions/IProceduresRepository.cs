using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Application.Applications.Abstractions;

public interface IProceduresRepository
{
    Task<IReadOnlyList<Procedimento>> ListByEmpresaIdAsync(
        Guid empresaId,
        bool includeInactive,
        Guid? produtoAplicadoId,
        string? search,
        int? limit,
        CancellationToken cancellationToken = default);

    Task<Procedimento?> GetByIdAndEmpresaIdWithDetailsAsync(
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

    Task<bool> AllActiveProductsExistAsync(
        Guid empresaId,
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken = default);

    Task AddAsync(Procedimento procedimento, CancellationToken cancellationToken = default);

    void Update(Procedimento procedimento);
}
