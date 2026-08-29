using AG.CLINICAL.Domain.Entities;
using AG.CLINICAL.Domain.Enums;

namespace AG.CLINICAL.Application.ClinicalRecords.Abstractions;

public interface IMedicalRecordsRepository
{
    Task<Prontuario?> GetByPacienteIdAsync(Guid empresaId, Guid pacienteId, CancellationToken cancellationToken = default);

    Task AddAsync(Prontuario prontuario, CancellationToken cancellationToken = default);

    void Update(Prontuario prontuario);

    Task AddEventoAsync(EventoClinico evento, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EventoClinico>> ListEventosByPacienteAsync(
        Guid empresaId,
        Guid pacienteId,
        int limit,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EventoClinico>> ListEventosByAtendimentoAsync(
        Guid empresaId,
        Guid atendimentoId,
        CancellationToken cancellationToken = default);
}

public interface IClinicalEncountersRepository
{
    Task<IReadOnlyList<AtendimentoClinico>> ListByPacienteAsync(
        Guid empresaId,
        Guid pacienteId,
        bool includeInactive,
        CancellationToken cancellationToken = default);

    Task<AtendimentoClinico?> GetByIdAndEmpresaIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default);

    Task<bool> HasOpenEncounterAsync(
        Guid empresaId,
        Guid pacienteId,
        Guid? excludeId,
        CancellationToken cancellationToken = default);

    Task AddAsync(AtendimentoClinico atendimento, CancellationToken cancellationToken = default);

    void Update(AtendimentoClinico atendimento);
}

public interface IClinicalNotesRepository
{
    Task<IReadOnlyList<AnotacaoClinica>> ListByAtendimentoAsync(
        Guid empresaId,
        Guid atendimentoId,
        CancellationToken cancellationToken = default);

    Task<AnotacaoClinica?> GetByIdAndEmpresaIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default);

    Task AddAsync(AnotacaoClinica anotacao, CancellationToken cancellationToken = default);

    void Update(AnotacaoClinica anotacao);
}

public interface IAnamneseTemplatesRepository
{
    Task<IReadOnlyList<ModeloAnamnese>> ListByEmpresaIdAsync(
        Guid empresaId,
        bool includeInactive,
        CancellationToken cancellationToken = default);

    Task<ModeloAnamnese?> GetByIdAndEmpresaIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByNomeAsync(
        Guid empresaId,
        string nome,
        Guid? excludeId,
        CancellationToken cancellationToken = default);

    Task AddAsync(ModeloAnamnese modelo, CancellationToken cancellationToken = default);

    void Update(ModeloAnamnese modelo);
}

public interface IAnamneseRecordsRepository
{
    Task<IReadOnlyList<RegistroAnamnese>> ListByAtendimentoAsync(
        Guid empresaId,
        Guid atendimentoId,
        CancellationToken cancellationToken = default);

    Task<RegistroAnamnese?> GetByIdAndEmpresaIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default);

    Task AddAsync(RegistroAnamnese registro, CancellationToken cancellationToken = default);

    void Update(RegistroAnamnese registro);
}

public interface IBodyAssessmentsRepository
{
    Task<IReadOnlyList<AvaliacaoCorporal>> ListByPacienteAsync(
        Guid empresaId,
        Guid pacienteId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AvaliacaoCorporal>> ListByAtendimentoAsync(
        Guid empresaId,
        Guid atendimentoId,
        CancellationToken cancellationToken = default);

    Task<AvaliacaoCorporal?> GetByIdAndEmpresaIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default);

    Task<AvaliacaoCorporal?> GetLatestByPacienteAsync(
        Guid empresaId,
        Guid pacienteId,
        CancellationToken cancellationToken = default);

    Task AddAsync(AvaliacaoCorporal avaliacao, CancellationToken cancellationToken = default);

    void Update(AvaliacaoCorporal avaliacao);
}

public interface IClinicalFilesRepository
{
    Task<IReadOnlyList<AnexoClinico>> ListAnexosByAtendimentoAsync(
        Guid empresaId,
        Guid atendimentoId,
        TipoAnexoClinico? tipo,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AnexoClinico>> ListAnexosByPacienteAsync(
        Guid empresaId,
        Guid pacienteId,
        TipoAnexoClinico? tipo,
        CancellationToken cancellationToken = default);

    Task<AnexoClinico?> GetAnexoByIdAsync(Guid id, Guid empresaId, CancellationToken cancellationToken = default);

    Task AddAnexoAsync(AnexoClinico anexo, CancellationToken cancellationToken = default);

    void UpdateAnexo(AnexoClinico anexo);

    Task<IReadOnlyList<FotoComparativa>> ListFotosByPacienteAsync(
        Guid empresaId,
        Guid pacienteId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FotoComparativa>> ListFotosByAtendimentoAsync(
        Guid empresaId,
        Guid atendimentoId,
        CancellationToken cancellationToken = default);

    Task<FotoComparativa?> GetFotoByIdAsync(Guid id, Guid empresaId, CancellationToken cancellationToken = default);

    Task AddFotoAsync(FotoComparativa foto, CancellationToken cancellationToken = default);

    void UpdateFoto(FotoComparativa foto);
}

public interface INutritionRecordsRepository
{
    Task<IReadOnlyList<CalculoEnergeticoRegistro>> ListVentaByAtendimentoAsync(
        Guid empresaId,
        Guid atendimentoId,
        CancellationToken cancellationToken = default);

    Task<CalculoEnergeticoRegistro?> GetVentaByIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default);

    Task AddVentaAsync(CalculoEnergeticoRegistro registro, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RegraBolsoRegistro>> ListRegraBolsoByAtendimentoAsync(
        Guid empresaId,
        Guid atendimentoId,
        CancellationToken cancellationToken = default);

    Task AddRegraBolsoAsync(RegraBolsoRegistro registro, CancellationToken cancellationToken = default);
}
