using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Application.Core.Abstractions;

public interface IUnitsRepository
{
    Task<IReadOnlyList<Unidade>> ListByEmpresaIdAsync(
        Guid empresaId,
        bool includeInactive,
        CancellationToken cancellationToken = default);

    Task<Unidade?> GetByIdAndEmpresaIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByNomeAsync(
        Guid empresaId,
        string nome,
        Guid? excludeId,
        CancellationToken cancellationToken = default);

    Task AddAsync(Unidade unidade, CancellationToken cancellationToken = default);

    void Update(Unidade unidade);
}
