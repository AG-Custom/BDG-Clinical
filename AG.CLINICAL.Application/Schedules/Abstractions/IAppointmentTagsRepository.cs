using AG.CLINICAL.Domain.Entities;

namespace AG.CLINICAL.Application.Schedules.Abstractions;

public interface IAppointmentTagsRepository
{
    Task<IReadOnlyList<TagAgendamento>> ListByEmpresaIdAsync(
        Guid empresaId,
        bool includeInactive,
        CancellationToken cancellationToken = default);

    Task<TagAgendamento?> GetByIdAndEmpresaIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TagAgendamento>> ListByIdsAndEmpresaIdAsync(
        Guid empresaId,
        IReadOnlyList<Guid> ids,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByNomeAsync(
        Guid empresaId,
        string nome,
        Guid? excludeId,
        CancellationToken cancellationToken = default);

    Task AddAsync(TagAgendamento tag, CancellationToken cancellationToken = default);

    void Update(TagAgendamento tag);
}
