using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Application.Patients.Abstractions;

public interface ISymptomsRepository
{
    Task<IReadOnlyList<Sintoma>> ListByEmpresaIdAsync(
        Guid empresaId,
        bool includeInactive,
        CancellationToken cancellationToken = default);

    Task<Sintoma?> GetByIdAndEmpresaIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByNomeAsync(
        Guid empresaId,
        string nome,
        Guid? excludeId,
        CancellationToken cancellationToken = default);

    Task<bool> AllExistActiveByIdsAsync(
        Guid empresaId,
        IReadOnlyList<Guid> sintomaIds,
        CancellationToken cancellationToken = default);

    Task AddAsync(Sintoma sintoma, CancellationToken cancellationToken = default);

    void Update(Sintoma sintoma);
}
