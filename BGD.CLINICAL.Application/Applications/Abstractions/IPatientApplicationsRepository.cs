using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Application.Applications.Abstractions;

public interface IPatientApplicationsRepository
{
    Task<AplicacaoPaciente?> GetByIdAndEmpresaIdWithDetailsAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AplicacaoPaciente>> ListByEmpresaIdAsync(
        Guid empresaId,
        Guid? pacienteId,
        Guid? unidadeId,
        Guid? produtoId,
        Guid? aplicadorId,
        bool? cancelada,
        DateTime? dataInicio,
        DateTime? dataFim,
        int limit,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsCompraPacienteForPacienteAsync(
        Guid compraPacienteId,
        Guid pacienteId,
        Guid empresaId,
        CancellationToken cancellationToken = default);

    Task AddAsync(AplicacaoPaciente aplicacao, CancellationToken cancellationToken = default);

    void Update(AplicacaoPaciente aplicacao);

    Task ReplaceSymptomsAsync(
        Guid aplicacaoPacienteId,
        IReadOnlyList<Guid> sintomaIds,
        CancellationToken cancellationToken = default);
}
