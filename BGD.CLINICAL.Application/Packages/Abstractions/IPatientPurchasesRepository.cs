using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;

namespace BGD.CLINICAL.Application.Packages.Abstractions;

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
}
