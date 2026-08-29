using AG.CLINICAL.Domain.Enums;

namespace AG.CLINICAL.Application.ClinicalRecords.Dtos;

public sealed record MedicalRecordDto(
    Guid Id,
    Guid PacienteId,
    string PacienteNome,
    string? PacienteSexo,
    DateOnly? PacienteDataNascimento,
    int? PacienteIdade,
    string? Alergias,
    string? Alertas,
    string? Observacao,
    DateTime CriadoEm,
    DateTime? AtualizadoEm);

public sealed record UpdateMedicalRecordRequest(
    string? Alergias,
    string? Alertas,
    string? Observacao);

public sealed record ClinicalEventDto(
    Guid Id,
    Guid? AtendimentoClinicoId,
    string Tipo,
    string Titulo,
    string? Resumo,
    string Entidade,
    Guid RegistroId,
    string? DadosAnteriores,
    string? DadosNovos,
    Guid FuncionarioId,
    string FuncionarioNome,
    Guid UnidadeId,
    string UnidadeNome,
    DateTime Data);

public sealed record ClinicalEncounterDto(
    Guid Id,
    Guid ProntuarioId,
    Guid PacienteId,
    string PacienteNome,
    Guid UnidadeId,
    string UnidadeNome,
    Guid FuncionarioId,
    string FuncionarioNome,
    DateTime DataInicio,
    DateTime? DataFim,
    string Status,
    string? Observacao,
    bool Ativo,
    DateTime CriadoEm,
    DateTime? AtualizadoEm,
    IReadOnlyList<ClinicalEventDto>? Timeline = null);

public sealed record CreateClinicalEncounterRequest(
    Guid UnidadeId,
    Guid FuncionarioId,
    DateTime? DataInicio = null,
    string? Observacao = null);

public sealed record UpdateClinicalEncounterRequest(
    Guid UnidadeId,
    Guid FuncionarioId,
    DateTime DataInicio,
    string? Observacao);

public sealed record ClinicalNoteDto(
    Guid Id,
    Guid AtendimentoClinicoId,
    string Tipo,
    string Texto,
    DateTime Data,
    Guid FuncionarioId,
    string FuncionarioNome,
    Guid UnidadeId,
    string UnidadeNome,
    DateTime CriadoEm,
    DateTime? AtualizadoEm);

public sealed record CreateClinicalNoteRequest(
    string Tipo,
    string Texto,
    DateTime? Data = null);

public sealed record UpdateClinicalNoteRequest(
    string Tipo,
    string Texto,
    DateTime Data);

public sealed record AnamneseFieldDto(
    string Id,
    string Tipo,
    string Label,
    bool Obrigatorio,
    IReadOnlyList<string>? Opcoes = null,
    decimal? Min = null,
    decimal? Max = null);

public sealed record AnamneseTemplateDto(
    Guid Id,
    string Nome,
    string? Especialidade,
    IReadOnlyList<AnamneseFieldDto> Campos,
    bool Ativo,
    DateTime CriadoEm,
    DateTime? AtualizadoEm);

public sealed record CreateAnamneseTemplateRequest(
    string Nome,
    string? Especialidade,
    IReadOnlyList<AnamneseFieldDto> Campos);

public sealed record UpdateAnamneseTemplateRequest(
    string Nome,
    string? Especialidade,
    IReadOnlyList<AnamneseFieldDto> Campos);

public sealed record AnamneseVersionDto(
    Guid Id,
    int Versao,
    Guid FuncionarioId,
    string FuncionarioNome,
    string RespostasJson,
    string? ResumoAlteracao,
    DateTime CriadoEm);

public sealed record AnamneseRecordDto(
    Guid Id,
    Guid AtendimentoClinicoId,
    Guid ModeloAnamneseId,
    string ModeloNome,
    string SchemaSnapshotJson,
    string RespostasJson,
    int VersaoAtual,
    Guid FuncionarioId,
    string FuncionarioNome,
    DateTime CriadoEm,
    DateTime? AtualizadoEm,
    IReadOnlyList<AnamneseVersionDto> Versoes);

public sealed record CreateAnamneseRecordRequest(
    Guid ModeloAnamneseId,
    string RespostasJson);

public sealed record UpdateAnamneseRecordRequest(
    string RespostasJson,
    string? ResumoAlteracao);

public sealed record BioimpedanciaDto(
    decimal? PercentualMassaGorda,
    decimal? MassaGordaKg,
    decimal? GorduraVisceral,
    decimal? PercentualMassaMagra,
    decimal? MassaMagraKg,
    decimal? PesoMuscularKg,
    decimal? PesoOsseoKg,
    decimal? PercentualAgua,
    decimal? AguaTotalKg,
    decimal? PesoResidualKg,
    decimal? IdadeMetabolica);

public sealed record CircunferenciasDto(
    decimal? BracoDireitoRelaxado,
    decimal? BracoDireitoContraido,
    decimal? AntebracoDireito,
    decimal? PunhoDireito,
    decimal? BracoEsquerdoRelaxado,
    decimal? BracoEsquerdoContraido,
    decimal? AntebracoEsquerdo,
    decimal? PunhoEsquerdo,
    decimal? Pescoco,
    decimal? Ombro,
    decimal? Torax,
    decimal? Abdomen,
    decimal? Cintura,
    decimal? Quadril,
    decimal? CoxaDireita,
    decimal? CoxaProximalDireita,
    decimal? PanturrilhaDireita,
    decimal? CoxaEsquerda,
    decimal? CoxaProximalEsquerda,
    decimal? PanturrilhaEsquerda);

public sealed record PregasDto(
    decimal? AxilarMedia,
    decimal? Triceps,
    decimal? Subescapular,
    decimal? SupraIliaca,
    decimal? Torax,
    decimal? Abdominal,
    decimal? Coxa);

public sealed record BodyAssessmentDto(
    Guid Id,
    Guid AtendimentoClinicoId,
    Guid PacienteId,
    Guid UnidadeId,
    string UnidadeNome,
    Guid FuncionarioId,
    string FuncionarioNome,
    DateTime DataAvaliacao,
    decimal? AlturaCm,
    decimal? PesoKg,
    decimal? Imc,
    string? ClassificacaoImc,
    decimal? PesoIdealKg,
    decimal? CinturaCm,
    decimal? QuadrilCm,
    decimal? RelacaoCinturaQuadril,
    decimal? PercentualMassaGorda,
    decimal? MassaGordaKg,
    decimal? PercentualMassaMagra,
    decimal? MassaMagraKg,
    decimal? PercentualAgua,
    string? ProtocoloPrega,
    BioimpedanciaDto? Bioimpedancia,
    CircunferenciasDto? Circunferencias,
    PregasDto? Pregas,
    string? Observacao,
    DateTime CriadoEm,
    DateTime? AtualizadoEm);

public sealed record UpsertBodyAssessmentRequest(
    DateTime? DataAvaliacao,
    decimal? AlturaCm,
    decimal? PesoKg,
    decimal? CinturaCm,
    decimal? QuadrilCm,
    string? ProtocoloPrega,
    BioimpedanciaDto? Bioimpedancia,
    CircunferenciasDto? Circunferencias,
    PregasDto? Pregas,
    string? Observacao);

public sealed record BodyEvolutionPointDto(
    DateTime Data,
    decimal? PesoKg,
    decimal? Imc,
    decimal? PercentualMassaGorda,
    decimal? MassaGordaKg,
    decimal? PercentualMassaMagra,
    decimal? MassaMagraKg,
    decimal? PercentualAgua,
    decimal? CinturaCm,
    decimal? QuadrilCm);

public sealed record ClinicalAttachmentDto(
    Guid Id,
    Guid AtendimentoClinicoId,
    string Tipo,
    string Nome,
    DateTime DataDocumento,
    string? Observacao,
    string? CategoriaExame,
    string? CategoriaDocumento,
    string NomeArquivo,
    string ContentType,
    string Url,
    long TamanhoBytes,
    Guid FuncionarioId,
    string FuncionarioNome,
    DateTime CriadoEm);

public sealed record UploadClinicalAttachmentRequest(
    string Tipo,
    string Nome,
    DateTime? DataDocumento,
    string? Observacao,
    string? CategoriaExame,
    string? CategoriaDocumento);

public sealed record ComparativePhotoDto(
    Guid Id,
    Guid AtendimentoClinicoId,
    Guid? AvaliacaoCorporalId,
    string Categoria,
    DateTime DataCaptura,
    string? Observacao,
    string NomeArquivo,
    string ContentType,
    string Url,
    long TamanhoBytes,
    Guid FuncionarioId,
    string FuncionarioNome,
    Guid UnidadeId,
    string UnidadeNome,
    DateTime CriadoEm);

public sealed record PhotoComparisonDto(
    ComparativePhotoDto Esquerda,
    ComparativePhotoDto Direita);

public sealed record AtividadeFisicaDto(
    string Nome,
    decimal Mets,
    decimal Minutos);

public sealed record EnergyCalculationDto(
    Guid Id,
    Guid AtendimentoClinicoId,
    string Perfil,
    string Protocolo,
    string NivelAtividade,
    decimal? FatorInjuria,
    decimal PesoKg,
    decimal AlturaCm,
    int Idade,
    decimal? MassaMagraKg,
    decimal PesoDesejadoKg,
    int TempoDias,
    IReadOnlyList<AtividadeFisicaDto> Atividades,
    decimal GastoEnergeticoBasal,
    decimal GastoEnergeticoTotal,
    decimal AjusteCaloricoDiario,
    decimal MetaCaloricaDiaria,
    DateTime CriadoEm);

public sealed record CreateEnergyCalculationRequest(
    string Perfil,
    string Protocolo,
    string NivelAtividade,
    decimal? FatorInjuria,
    decimal? PesoKg,
    decimal? AlturaCm,
    decimal? MassaMagraKg,
    decimal PesoDesejadoKg,
    int TempoDias,
    IReadOnlyList<AtividadeFisicaDto>? Atividades);

public sealed record PocketRuleDto(
    Guid Id,
    Guid AtendimentoClinicoId,
    string Objetivo,
    decimal PesoKg,
    decimal GastoEnergeticoTotal,
    decimal Calorias,
    decimal ProteinasG,
    decimal CarboidratosG,
    decimal GordurasG,
    DateTime CriadoEm);

public sealed record CreatePocketRuleRequest(
    string Objetivo,
    decimal? PesoKg,
    decimal? GastoEnergeticoTotal);

public sealed record NextAppointmentSummaryDto(
    Guid Id,
    DateTime DataInicio,
    string Tipo,
    string Status,
    string? FuncionarioNome,
    string? UnidadeNome);

public sealed record ActivePackageSummaryDto(
    Guid Id,
    string PacoteNome,
    DateTime DataCompra);

public sealed record ApplicationSummaryDto(
    Guid Id,
    DateTime DataAplicacao,
    string? ProcedimentoNome,
    string? ProdutoNome);

public sealed record MedicalRecordSummaryDto(
    MedicalRecordDto Prontuario,
    BodyAssessmentDto? UltimaAvaliacao,
    NextAppointmentSummaryDto? ProximoAgendamento,
    IReadOnlyList<ActivePackageSummaryDto> PacotesAtivos,
    IReadOnlyList<ApplicationSummaryDto> UltimasAplicacoes,
    IReadOnlyList<ClinicalEventDto> Timeline);
