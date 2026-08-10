using AG.CLINICAL.Domain.Entities;
using AG.CLINICAL.Domain.Enums;

namespace AG.CLINICAL.Application.Packages.Abstractions;

public interface IPatientPurchasesRepository
{
    Task<IReadOnlyList<CompraPaciente>> ListByEmpresaIdAsync(
        Guid empresaId,
        Guid? pacienteId,
        StatusCompraPaciente? status,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CompraPaciente>> ListByPacienteAsync(
        Guid empresaId,
        Guid pacienteId,
        StatusCompraPaciente? status,
        CancellationToken cancellationToken = default);

    Task<CompraPaciente?> GetByIdAndEmpresaIdWithDetailsAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CompraPaciente>> ListActiveByPacienteAsync(
        Guid empresaId,
        Guid pacienteId,
        CancellationToken cancellationToken = default);

    Task AddAsync(CompraPaciente compra, CancellationToken cancellationToken = default);

    void Update(CompraPaciente compra);

    Task<int> CountByPacoteIdAsync(
        Guid empresaId,
        Guid pacoteId,
        CancellationToken cancellationToken = default);
}
