using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Application.Patients.Abstractions;

public interface IPatientsRepository
{
    Task<IReadOnlyList<Paciente>> ListByEmpresaIdAsync(
        Guid empresaId,
        Guid? unidadeId,
        bool includeInactive,
        CancellationToken cancellationToken = default);

    Task<Paciente?> GetByIdAndEmpresaIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByCpfAsync(
        Guid empresaId,
        string cpf,
        Guid? excludeId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsActiveUnidadeInEmpresaAsync(
        Guid unidadeId,
        Guid empresaId,
        CancellationToken cancellationToken = default);

    Task AddAsync(Paciente paciente, CancellationToken cancellationToken = default);

    void Update(Paciente paciente);
}
