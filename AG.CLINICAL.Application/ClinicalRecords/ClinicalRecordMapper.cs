using System.Text.Json;
using AG.CLINICAL.Application.ClinicalRecords.Dtos;
using AG.CLINICAL.Domain.Entities;
using AG.CLINICAL.Domain.Services;

namespace AG.CLINICAL.Application.ClinicalRecords;

internal static class ClinicalRecordJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public static string Serialize<T>(T value) => JsonSerializer.Serialize(value, Options);

    public static T? Deserialize<T>(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(json, Options);
    }
}

internal static class ClinicalRecordMapper
{
    public static MedicalRecordDto Map(Prontuario prontuario, DateOnly hoje)
    {
        var paciente = prontuario.Paciente;
        return new MedicalRecordDto(
            prontuario.Id,
            prontuario.PacienteId,
            paciente?.Nome ?? string.Empty,
            paciente?.Sexo?.ToString(),
            paciente?.DataNascimento,
            CalculoCorporal.CalcularIdade(paciente?.DataNascimento, hoje),
            prontuario.Alergias,
            prontuario.Alertas,
            prontuario.Observacao,
            prontuario.CriadoEm,
            prontuario.AtualizadoEm);
    }

    public static ClinicalEventDto Map(EventoClinico evento)
    {
        return new ClinicalEventDto(
            evento.Id,
            evento.AtendimentoClinicoId,
            evento.Tipo.ToString(),
            evento.Titulo,
            evento.Resumo,
            evento.Entidade,
            evento.RegistroId,
            evento.DadosAnteriores,
            evento.DadosNovos,
            evento.FuncionarioId,
            evento.Funcionario?.Nome ?? string.Empty,
            evento.UnidadeId,
            evento.Unidade?.Nome ?? string.Empty,
            evento.Data);
    }

    public static ClinicalEncounterDto Map(AtendimentoClinico atendimento, IReadOnlyList<ClinicalEventDto>? timeline = null)
    {
        return new ClinicalEncounterDto(
            atendimento.Id,
            atendimento.ProntuarioId,
            atendimento.PacienteId,
            atendimento.Paciente?.Nome ?? string.Empty,
            atendimento.UnidadeId,
            atendimento.Unidade?.Nome ?? string.Empty,
            atendimento.FuncionarioId,
            atendimento.Funcionario?.Nome ?? string.Empty,
            atendimento.DataInicio,
            atendimento.DataFim,
            atendimento.Status.ToString(),
            atendimento.Observacao,
            atendimento.Ativo,
            atendimento.CriadoEm,
            atendimento.AtualizadoEm,
            timeline);
    }

    public static ClinicalNoteDto Map(AnotacaoClinica anotacao)
    {
        return new ClinicalNoteDto(
            anotacao.Id,
            anotacao.AtendimentoClinicoId,
            anotacao.Tipo.ToString(),
            anotacao.Texto,
            anotacao.Data,
            anotacao.FuncionarioId,
            anotacao.Funcionario?.Nome ?? string.Empty,
            anotacao.UnidadeId,
            anotacao.Unidade?.Nome ?? string.Empty,
            anotacao.CriadoEm,
            anotacao.AtualizadoEm);
    }

    public static AnamneseTemplateDto Map(ModeloAnamnese modelo)
    {
        var campos = ClinicalRecordJson.Deserialize<List<AnamneseFieldDto>>(modelo.SchemaJson) ?? [];
        return new AnamneseTemplateDto(
            modelo.Id,
            modelo.Nome,
            modelo.Especialidade,
            campos,
            modelo.Ativo,
            modelo.CriadoEm,
            modelo.AtualizadoEm);
    }

    public static AnamneseRecordDto Map(RegistroAnamnese registro)
    {
        var versoes = registro.Versoes
            .OrderByDescending(item => item.Versao)
            .Select(item => new AnamneseVersionDto(
                item.Id,
                item.Versao,
                item.FuncionarioId,
                item.Funcionario?.Nome ?? string.Empty,
                item.RespostasJson,
                item.ResumoAlteracao,
                item.CriadoEm))
            .ToList();

        return new AnamneseRecordDto(
            registro.Id,
            registro.AtendimentoClinicoId,
            registro.ModeloAnamneseId,
            registro.ModeloAnamnese?.Nome ?? string.Empty,
            registro.SchemaSnapshotJson,
            registro.RespostasJson,
            registro.VersaoAtual,
            registro.FuncionarioId,
            registro.Funcionario?.Nome ?? string.Empty,
            registro.CriadoEm,
            registro.AtualizadoEm,
            versoes);
    }

    public static BodyAssessmentDto Map(AvaliacaoCorporal avaliacao)
    {
        return new BodyAssessmentDto(
            avaliacao.Id,
            avaliacao.AtendimentoClinicoId,
            avaliacao.PacienteId,
            avaliacao.UnidadeId,
            avaliacao.Unidade?.Nome ?? string.Empty,
            avaliacao.FuncionarioId,
            avaliacao.Funcionario?.Nome ?? string.Empty,
            avaliacao.DataAvaliacao,
            avaliacao.AlturaCm,
            avaliacao.PesoKg,
            avaliacao.Imc,
            avaliacao.ClassificacaoImc,
            avaliacao.PesoIdealKg,
            avaliacao.CinturaCm,
            avaliacao.QuadrilCm,
            avaliacao.RelacaoCinturaQuadril,
            avaliacao.PercentualMassaGorda,
            avaliacao.MassaGordaKg,
            avaliacao.PercentualMassaMagra,
            avaliacao.MassaMagraKg,
            avaliacao.PercentualAgua,
            avaliacao.ProtocoloPrega?.ToString(),
            ClinicalRecordJson.Deserialize<BioimpedanciaDto>(avaliacao.BioimpedanciaJson),
            ClinicalRecordJson.Deserialize<CircunferenciasDto>(avaliacao.CircunferenciasJson),
            ClinicalRecordJson.Deserialize<PregasDto>(avaliacao.PregasJson),
            avaliacao.Observacao,
            avaliacao.CriadoEm,
            avaliacao.AtualizadoEm);
    }

    public static ClinicalAttachmentDto Map(AnexoClinico anexo, string url)
    {
        return new ClinicalAttachmentDto(
            anexo.Id,
            anexo.AtendimentoClinicoId,
            anexo.Tipo.ToString(),
            anexo.Nome,
            anexo.DataDocumento,
            anexo.Observacao,
            anexo.CategoriaExame,
            anexo.CategoriaDocumento,
            anexo.NomeArquivo,
            anexo.ContentType,
            url,
            anexo.TamanhoBytes,
            anexo.FuncionarioId,
            anexo.Funcionario?.Nome ?? string.Empty,
            anexo.CriadoEm);
    }

    public static ComparativePhotoDto Map(FotoComparativa foto, string url)
    {
        return new ComparativePhotoDto(
            foto.Id,
            foto.AtendimentoClinicoId,
            foto.AvaliacaoCorporalId,
            foto.Categoria.ToString(),
            foto.DataCaptura,
            foto.Observacao,
            foto.NomeArquivo,
            foto.ContentType,
            url,
            foto.TamanhoBytes,
            foto.FuncionarioId,
            foto.Funcionario?.Nome ?? string.Empty,
            foto.UnidadeId,
            foto.Unidade?.Nome ?? string.Empty,
            foto.CriadoEm);
    }

    public static EnergyCalculationDto Map(CalculoEnergeticoRegistro registro)
    {
        var atividades = ClinicalRecordJson.Deserialize<List<AtividadeFisicaDto>>(registro.AtividadesJson) ?? [];
        return new EnergyCalculationDto(
            registro.Id,
            registro.AtendimentoClinicoId,
            registro.Perfil.ToString(),
            registro.Protocolo.ToString(),
            registro.NivelAtividade.ToString(),
            registro.FatorInjuria,
            registro.PesoKg,
            registro.AlturaCm,
            registro.Idade,
            registro.MassaMagraKg,
            registro.PesoDesejadoKg,
            registro.TempoDias,
            atividades,
            registro.GastoEnergeticoBasal,
            registro.GastoEnergeticoTotal,
            registro.AjusteCaloricoDiario,
            registro.MetaCaloricaDiaria,
            registro.CriadoEm);
    }

    public static PocketRuleDto Map(RegraBolsoRegistro registro)
    {
        return new PocketRuleDto(
            registro.Id,
            registro.AtendimentoClinicoId,
            registro.Objetivo.ToString(),
            registro.PesoKg,
            registro.GastoEnergeticoTotal,
            registro.Calorias,
            registro.ProteinasG,
            registro.CarboidratosG,
            registro.GordurasG,
            registro.CriadoEm);
    }
}
