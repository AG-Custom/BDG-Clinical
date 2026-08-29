using AG.CLINICAL.Domain.Common;
using AG.CLINICAL.Domain.Enums;
using AG.CLINICAL.Domain.Exceptions;

namespace AG.CLINICAL.Domain.Entities;

public sealed class Prontuario : AggregateRoot
{
    private Prontuario()
    {
    }

    private Prontuario(Guid empresaId, Guid pacienteId, string? alergias, string? alertas, string? observacao)
        : base(Guid.NewGuid())
    {
        EmpresaId = empresaId;
        PacienteId = pacienteId;
        Alergias = alergias;
        Alertas = alertas;
        Observacao = observacao;
        Ativo = true;
    }

    public Guid EmpresaId { get; private set; }
    public Guid PacienteId { get; private set; }
    public string? Alergias { get; private set; }
    public string? Alertas { get; private set; }
    public string? Observacao { get; private set; }
    public bool Ativo { get; private set; }

    public Empresa Empresa { get; private set; } = null!;
    public Paciente Paciente { get; private set; } = null!;
    public ICollection<AtendimentoClinico> Atendimentos { get; private set; } = [];

    public static Prontuario Create(Guid empresaId, Guid pacienteId)
    {
        if (empresaId == Guid.Empty)
        {
            throw new DomainException("Informe a empresa do prontuário.");
        }

        if (pacienteId == Guid.Empty)
        {
            throw new DomainException("Informe o paciente do prontuário.");
        }

        return new Prontuario(empresaId, pacienteId, null, null, null);
    }

    public void UpdateDetails(string? alergias, string? alertas, string? observacao)
    {
        Alergias = NormalizeOptional(alergias, 4000);
        Alertas = NormalizeOptional(alertas, 4000);
        Observacao = NormalizeOptional(observacao, 4000);
        AtualizadoEm = DateTime.UtcNow;
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new DomainException($"O texto não pode ultrapassar {maxLength} caracteres.");
        }

        return trimmed;
    }
}

public sealed class AtendimentoClinico : AggregateRoot
{
    private AtendimentoClinico()
    {
    }

    private AtendimentoClinico(
        Guid empresaId,
        Guid prontuarioId,
        Guid pacienteId,
        Guid unidadeId,
        Guid funcionarioId,
        DateTime dataInicio,
        string? observacao,
        Guid? agendamentoId)
        : base(Guid.NewGuid())
    {
        EmpresaId = empresaId;
        ProntuarioId = prontuarioId;
        PacienteId = pacienteId;
        UnidadeId = unidadeId;
        FuncionarioId = funcionarioId;
        AgendamentoId = agendamentoId;
        DataInicio = dataInicio;
        Observacao = observacao;
        Status = StatusAtendimentoClinico.EmAndamento;
        Ativo = true;
    }

    public Guid EmpresaId { get; private set; }
    public Guid ProntuarioId { get; private set; }
    public Guid PacienteId { get; private set; }
    public Guid UnidadeId { get; private set; }
    public Guid FuncionarioId { get; private set; }
    public Guid? AgendamentoId { get; private set; }
    public DateTime DataInicio { get; private set; }
    public DateTime? DataFim { get; private set; }
    public StatusAtendimentoClinico Status { get; private set; }
    public string? Observacao { get; private set; }
    public bool Ativo { get; private set; }

    public Empresa Empresa { get; private set; } = null!;
    public Prontuario Prontuario { get; private set; } = null!;
    public Paciente Paciente { get; private set; } = null!;
    public Unidade Unidade { get; private set; } = null!;
    public Funcionario Funcionario { get; private set; } = null!;
    public ICollection<AnotacaoClinica> Anotacoes { get; private set; } = [];
    public ICollection<RegistroAnamnese> Anamneses { get; private set; } = [];
    public ICollection<AvaliacaoCorporal> Avaliacoes { get; private set; } = [];
    public ICollection<AnexoClinico> Anexos { get; private set; } = [];
    public ICollection<FotoComparativa> Fotos { get; private set; } = [];
    public ICollection<CalculoEnergeticoRegistro> CalculosEnergeticos { get; private set; } = [];
    public ICollection<RegraBolsoRegistro> RegrasBolso { get; private set; } = [];
    public ICollection<EventoClinico> Eventos { get; private set; } = [];

    public static AtendimentoClinico Create(
        Guid empresaId,
        Guid prontuarioId,
        Guid pacienteId,
        Guid unidadeId,
        Guid funcionarioId,
        DateTime dataInicio,
        string? observacao,
        Guid? agendamentoId = null)
    {
        ValidateIds(empresaId, prontuarioId, pacienteId, unidadeId, funcionarioId);

        return new AtendimentoClinico(
            empresaId,
            prontuarioId,
            pacienteId,
            unidadeId,
            funcionarioId,
            dataInicio,
            NormalizeOptional(observacao, 4000),
            agendamentoId is null || agendamentoId == Guid.Empty ? null : agendamentoId);
    }

    public void UpdateDetails(Guid unidadeId, Guid funcionarioId, DateTime dataInicio, string? observacao)
    {
        if (unidadeId == Guid.Empty)
        {
            throw new DomainException("Informe a unidade do atendimento.");
        }

        if (funcionarioId == Guid.Empty)
        {
            throw new DomainException("Informe o profissional do atendimento.");
        }

        UnidadeId = unidadeId;
        FuncionarioId = funcionarioId;
        DataInicio = dataInicio;
        Observacao = NormalizeOptional(observacao, 4000);
        AtualizadoEm = DateTime.UtcNow;
    }

    public void FinalizeEncounter(DateTime? dataFim = null)
    {
        Status = StatusAtendimentoClinico.Finalizado;
        DataFim = dataFim ?? DateTime.UtcNow;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void Reopen()
    {
        Status = StatusAtendimentoClinico.EmAndamento;
        DataFim = null;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        Ativo = false;
        AtualizadoEm = DateTime.UtcNow;
    }

    private static void ValidateIds(
        Guid empresaId,
        Guid prontuarioId,
        Guid pacienteId,
        Guid unidadeId,
        Guid funcionarioId)
    {
        if (empresaId == Guid.Empty)
        {
            throw new DomainException("Informe a empresa do atendimento.");
        }

        if (prontuarioId == Guid.Empty)
        {
            throw new DomainException("Informe o prontuário do atendimento.");
        }

        if (pacienteId == Guid.Empty)
        {
            throw new DomainException("Informe o paciente do atendimento.");
        }

        if (unidadeId == Guid.Empty)
        {
            throw new DomainException("Informe a unidade do atendimento.");
        }

        if (funcionarioId == Guid.Empty)
        {
            throw new DomainException("Informe o profissional do atendimento.");
        }
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new DomainException($"O texto não pode ultrapassar {maxLength} caracteres.");
        }

        return trimmed;
    }
}

public sealed class EventoClinico : Entity
{
    private EventoClinico()
    {
    }

    private EventoClinico(
        Guid empresaId,
        Guid pacienteId,
        Guid? atendimentoClinicoId,
        TipoEventoClinico tipo,
        string titulo,
        string? resumo,
        string entidade,
        Guid registroId,
        string? dadosAnteriores,
        string? dadosNovos,
        Guid funcionarioId,
        Guid unidadeId)
        : base(Guid.NewGuid())
    {
        EmpresaId = empresaId;
        PacienteId = pacienteId;
        AtendimentoClinicoId = atendimentoClinicoId;
        Tipo = tipo;
        Titulo = titulo;
        Resumo = resumo;
        Entidade = entidade;
        RegistroId = registroId;
        DadosAnteriores = dadosAnteriores;
        DadosNovos = dadosNovos;
        FuncionarioId = funcionarioId;
        UnidadeId = unidadeId;
        Data = DateTime.UtcNow;
    }

    public Guid EmpresaId { get; private set; }
    public Guid PacienteId { get; private set; }
    public Guid? AtendimentoClinicoId { get; private set; }
    public TipoEventoClinico Tipo { get; private set; }
    public string Titulo { get; private set; } = string.Empty;
    public string? Resumo { get; private set; }
    public string Entidade { get; private set; } = string.Empty;
    public Guid RegistroId { get; private set; }
    public string? DadosAnteriores { get; private set; }
    public string? DadosNovos { get; private set; }
    public Guid FuncionarioId { get; private set; }
    public Guid UnidadeId { get; private set; }
    public DateTime Data { get; private set; }

    public Empresa Empresa { get; private set; } = null!;
    public Paciente Paciente { get; private set; } = null!;
    public AtendimentoClinico? AtendimentoClinico { get; private set; }
    public Funcionario Funcionario { get; private set; } = null!;
    public Unidade Unidade { get; private set; } = null!;

    public static EventoClinico Create(
        Guid empresaId,
        Guid pacienteId,
        Guid? atendimentoClinicoId,
        TipoEventoClinico tipo,
        string titulo,
        string? resumo,
        string entidade,
        Guid registroId,
        Guid funcionarioId,
        Guid unidadeId,
        string? dadosAnteriores = null,
        string? dadosNovos = null)
    {
        if (string.IsNullOrWhiteSpace(titulo))
        {
            throw new DomainException("Informe o título do evento clínico.");
        }

        return new EventoClinico(
            empresaId,
            pacienteId,
            atendimentoClinicoId,
            tipo,
            titulo.Trim(),
            string.IsNullOrWhiteSpace(resumo) ? null : resumo.Trim(),
            entidade,
            registroId,
            dadosAnteriores,
            dadosNovos,
            funcionarioId,
            unidadeId);
    }
}

public sealed class AnotacaoClinica : AggregateRoot
{
    private AnotacaoClinica()
    {
    }

    private AnotacaoClinica(
        Guid empresaId,
        Guid atendimentoClinicoId,
        Guid pacienteId,
        Guid unidadeId,
        Guid funcionarioId,
        TipoAnotacaoClinica tipo,
        string texto,
        DateTime data)
        : base(Guid.NewGuid())
    {
        EmpresaId = empresaId;
        AtendimentoClinicoId = atendimentoClinicoId;
        PacienteId = pacienteId;
        UnidadeId = unidadeId;
        FuncionarioId = funcionarioId;
        Tipo = tipo;
        Texto = texto;
        Data = data;
        Ativo = true;
    }

    public Guid EmpresaId { get; private set; }
    public Guid AtendimentoClinicoId { get; private set; }
    public Guid PacienteId { get; private set; }
    public Guid UnidadeId { get; private set; }
    public Guid FuncionarioId { get; private set; }
    public TipoAnotacaoClinica Tipo { get; private set; }
    public string Texto { get; private set; } = string.Empty;
    public DateTime Data { get; private set; }
    public bool Ativo { get; private set; }

    public Empresa Empresa { get; private set; } = null!;
    public AtendimentoClinico AtendimentoClinico { get; private set; } = null!;
    public Paciente Paciente { get; private set; } = null!;
    public Unidade Unidade { get; private set; } = null!;
    public Funcionario Funcionario { get; private set; } = null!;

    public static AnotacaoClinica Create(
        Guid empresaId,
        Guid atendimentoClinicoId,
        Guid pacienteId,
        Guid unidadeId,
        Guid funcionarioId,
        TipoAnotacaoClinica tipo,
        string texto,
        DateTime data)
    {
        ValidateRequired(empresaId, atendimentoClinicoId, pacienteId, unidadeId, funcionarioId, texto);

        return new AnotacaoClinica(
            empresaId,
            atendimentoClinicoId,
            pacienteId,
            unidadeId,
            funcionarioId,
            tipo,
            texto.Trim(),
            data);
    }

    public void UpdateDetails(TipoAnotacaoClinica tipo, string texto, DateTime data)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            throw new DomainException("Informe o texto da anotação clínica.");
        }

        Tipo = tipo;
        Texto = texto.Trim();
        Data = data;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        Ativo = false;
        AtualizadoEm = DateTime.UtcNow;
    }

    private static void ValidateRequired(
        Guid empresaId,
        Guid atendimentoClinicoId,
        Guid pacienteId,
        Guid unidadeId,
        Guid funcionarioId,
        string texto)
    {
        if (empresaId == Guid.Empty || atendimentoClinicoId == Guid.Empty || pacienteId == Guid.Empty)
        {
            throw new DomainException("Informe o atendimento da anotação clínica.");
        }

        if (unidadeId == Guid.Empty || funcionarioId == Guid.Empty)
        {
            throw new DomainException("Informe a unidade e o profissional da anotação.");
        }

        if (string.IsNullOrWhiteSpace(texto))
        {
            throw new DomainException("Informe o texto da anotação clínica.");
        }
    }
}

public sealed class ModeloAnamnese : AggregateRoot
{
    private ModeloAnamnese()
    {
    }

    private ModeloAnamnese(Guid empresaId, string nome, string? especialidade, string schemaJson)
        : base(Guid.NewGuid())
    {
        EmpresaId = empresaId;
        Nome = nome;
        Especialidade = especialidade;
        SchemaJson = schemaJson;
        Ativo = true;
    }

    public Guid EmpresaId { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string? Especialidade { get; private set; }
    public string SchemaJson { get; private set; } = "[]";
    public bool Ativo { get; private set; }

    public Empresa Empresa { get; private set; } = null!;

    public static ModeloAnamnese Create(Guid empresaId, string nome, string? especialidade, string schemaJson)
    {
        if (empresaId == Guid.Empty)
        {
            throw new DomainException("Informe a empresa do modelo de anamnese.");
        }

        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DomainException("Informe o nome do modelo de anamnese.");
        }

        return new ModeloAnamnese(
            empresaId,
            nome.Trim(),
            string.IsNullOrWhiteSpace(especialidade) ? null : especialidade.Trim(),
            string.IsNullOrWhiteSpace(schemaJson) ? "[]" : schemaJson);
    }

    public void UpdateDetails(string nome, string? especialidade, string schemaJson)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DomainException("Informe o nome do modelo de anamnese.");
        }

        Nome = nome.Trim();
        Especialidade = string.IsNullOrWhiteSpace(especialidade) ? null : especialidade.Trim();
        SchemaJson = string.IsNullOrWhiteSpace(schemaJson) ? "[]" : schemaJson;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        Ativo = false;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void Reactivate()
    {
        Ativo = true;
        AtualizadoEm = DateTime.UtcNow;
    }
}

public sealed class RegistroAnamnese : AggregateRoot
{
    private RegistroAnamnese()
    {
    }

    private RegistroAnamnese(
        Guid empresaId,
        Guid atendimentoClinicoId,
        Guid pacienteId,
        Guid unidadeId,
        Guid funcionarioId,
        Guid modeloAnamneseId,
        string schemaSnapshotJson,
        string respostasJson)
        : base(Guid.NewGuid())
    {
        EmpresaId = empresaId;
        AtendimentoClinicoId = atendimentoClinicoId;
        PacienteId = pacienteId;
        UnidadeId = unidadeId;
        FuncionarioId = funcionarioId;
        ModeloAnamneseId = modeloAnamneseId;
        SchemaSnapshotJson = schemaSnapshotJson;
        RespostasJson = respostasJson;
        VersaoAtual = 1;
        Ativo = true;
    }

    public Guid EmpresaId { get; private set; }
    public Guid AtendimentoClinicoId { get; private set; }
    public Guid PacienteId { get; private set; }
    public Guid UnidadeId { get; private set; }
    public Guid FuncionarioId { get; private set; }
    public Guid ModeloAnamneseId { get; private set; }
    public string SchemaSnapshotJson { get; private set; } = "[]";
    public string RespostasJson { get; private set; } = "{}";
    public int VersaoAtual { get; private set; }
    public bool Ativo { get; private set; }

    public Empresa Empresa { get; private set; } = null!;
    public AtendimentoClinico AtendimentoClinico { get; private set; } = null!;
    public Paciente Paciente { get; private set; } = null!;
    public Unidade Unidade { get; private set; } = null!;
    public Funcionario Funcionario { get; private set; } = null!;
    public ModeloAnamnese ModeloAnamnese { get; private set; } = null!;
    public ICollection<RegistroAnamneseVersao> Versoes { get; private set; } = [];

    public static RegistroAnamnese Create(
        Guid empresaId,
        Guid atendimentoClinicoId,
        Guid pacienteId,
        Guid unidadeId,
        Guid funcionarioId,
        Guid modeloAnamneseId,
        string schemaSnapshotJson,
        string respostasJson)
    {
        if (modeloAnamneseId == Guid.Empty)
        {
            throw new DomainException("Selecione um modelo de anamnese.");
        }

        var registro = new RegistroAnamnese(
            empresaId,
            atendimentoClinicoId,
            pacienteId,
            unidadeId,
            funcionarioId,
            modeloAnamneseId,
            string.IsNullOrWhiteSpace(schemaSnapshotJson) ? "[]" : schemaSnapshotJson,
            string.IsNullOrWhiteSpace(respostasJson) ? "{}" : respostasJson);

        registro.Versoes.Add(RegistroAnamneseVersao.Create(
            registro.Id,
            1,
            funcionarioId,
            registro.RespostasJson,
            null));

        return registro;
    }

    public void UpdateAnswers(Guid funcionarioId, string respostasJson, string? resumoAlteracao)
    {
        var anteriores = RespostasJson;
        RespostasJson = string.IsNullOrWhiteSpace(respostasJson) ? "{}" : respostasJson;
        FuncionarioId = funcionarioId;
        VersaoAtual += 1;
        AtualizadoEm = DateTime.UtcNow;

        Versoes.Add(RegistroAnamneseVersao.Create(
            Id,
            VersaoAtual,
            funcionarioId,
            RespostasJson,
            resumoAlteracao,
            anteriores));
    }

    public void Deactivate()
    {
        Ativo = false;
        AtualizadoEm = DateTime.UtcNow;
    }
}

public sealed class RegistroAnamneseVersao : Entity
{
    private RegistroAnamneseVersao()
    {
    }

    private RegistroAnamneseVersao(
        Guid registroAnamneseId,
        int versao,
        Guid funcionarioId,
        string respostasJson,
        string? resumoAlteracao,
        string? respostasAnterioresJson)
        : base(Guid.NewGuid())
    {
        RegistroAnamneseId = registroAnamneseId;
        Versao = versao;
        FuncionarioId = funcionarioId;
        RespostasJson = respostasJson;
        ResumoAlteracao = resumoAlteracao;
        RespostasAnterioresJson = respostasAnterioresJson;
    }

    public Guid RegistroAnamneseId { get; private set; }
    public int Versao { get; private set; }
    public Guid FuncionarioId { get; private set; }
    public string RespostasJson { get; private set; } = "{}";
    public string? ResumoAlteracao { get; private set; }
    public string? RespostasAnterioresJson { get; private set; }

    public RegistroAnamnese RegistroAnamnese { get; private set; } = null!;
    public Funcionario Funcionario { get; private set; } = null!;

    public static RegistroAnamneseVersao Create(
        Guid registroAnamneseId,
        int versao,
        Guid funcionarioId,
        string respostasJson,
        string? resumoAlteracao,
        string? respostasAnterioresJson = null)
    {
        return new RegistroAnamneseVersao(
            registroAnamneseId,
            versao,
            funcionarioId,
            respostasJson,
            string.IsNullOrWhiteSpace(resumoAlteracao) ? null : resumoAlteracao.Trim(),
            respostasAnterioresJson);
    }
}

public sealed class AvaliacaoCorporal : AggregateRoot
{
    private AvaliacaoCorporal()
    {
    }

    private AvaliacaoCorporal(
        Guid empresaId,
        Guid atendimentoClinicoId,
        Guid pacienteId,
        Guid unidadeId,
        Guid funcionarioId,
        DateTime dataAvaliacao)
        : base(Guid.NewGuid())
    {
        EmpresaId = empresaId;
        AtendimentoClinicoId = atendimentoClinicoId;
        PacienteId = pacienteId;
        UnidadeId = unidadeId;
        FuncionarioId = funcionarioId;
        DataAvaliacao = dataAvaliacao;
        Ativo = true;
    }

    public Guid EmpresaId { get; private set; }
    public Guid AtendimentoClinicoId { get; private set; }
    public Guid PacienteId { get; private set; }
    public Guid UnidadeId { get; private set; }
    public Guid FuncionarioId { get; private set; }
    public DateTime DataAvaliacao { get; private set; }
    public decimal? AlturaCm { get; private set; }
    public decimal? PesoKg { get; private set; }
    public decimal? Imc { get; private set; }
    public string? ClassificacaoImc { get; private set; }
    public decimal? PesoIdealKg { get; private set; }
    public decimal? CinturaCm { get; private set; }
    public decimal? QuadrilCm { get; private set; }
    public decimal? RelacaoCinturaQuadril { get; private set; }
    public decimal? PercentualMassaGorda { get; private set; }
    public decimal? MassaGordaKg { get; private set; }
    public decimal? PercentualMassaMagra { get; private set; }
    public decimal? MassaMagraKg { get; private set; }
    public decimal? PercentualAgua { get; private set; }
    public ProtocoloPregaCutanea? ProtocoloPrega { get; private set; }
    public string? BioimpedanciaJson { get; private set; }
    public string? CircunferenciasJson { get; private set; }
    public string? PregasJson { get; private set; }
    public string? Observacao { get; private set; }
    public bool Ativo { get; private set; }

    public Empresa Empresa { get; private set; } = null!;
    public AtendimentoClinico AtendimentoClinico { get; private set; } = null!;
    public Paciente Paciente { get; private set; } = null!;
    public Unidade Unidade { get; private set; } = null!;
    public Funcionario Funcionario { get; private set; } = null!;

    public static AvaliacaoCorporal Create(
        Guid empresaId,
        Guid atendimentoClinicoId,
        Guid pacienteId,
        Guid unidadeId,
        Guid funcionarioId,
        DateTime dataAvaliacao)
    {
        if (atendimentoClinicoId == Guid.Empty)
        {
            throw new DomainException("Informe o atendimento da avaliação corporal.");
        }

        return new AvaliacaoCorporal(
            empresaId,
            atendimentoClinicoId,
            pacienteId,
            unidadeId,
            funcionarioId,
            dataAvaliacao);
    }

    public void ApplyMeasurements(
        decimal? alturaCm,
        decimal? pesoKg,
        decimal? imc,
        string? classificacaoImc,
        decimal? pesoIdealKg,
        decimal? cinturaCm,
        decimal? quadrilCm,
        decimal? relacaoCinturaQuadril,
        decimal? percentualMassaGorda,
        decimal? massaGordaKg,
        decimal? percentualMassaMagra,
        decimal? massaMagraKg,
        decimal? percentualAgua,
        ProtocoloPregaCutanea? protocoloPrega,
        string? bioimpedanciaJson,
        string? circunferenciasJson,
        string? pregasJson,
        string? observacao)
    {
        AlturaCm = alturaCm;
        PesoKg = pesoKg;
        Imc = imc;
        ClassificacaoImc = classificacaoImc;
        PesoIdealKg = pesoIdealKg;
        CinturaCm = cinturaCm;
        QuadrilCm = quadrilCm;
        RelacaoCinturaQuadril = relacaoCinturaQuadril;
        PercentualMassaGorda = percentualMassaGorda;
        MassaGordaKg = massaGordaKg;
        PercentualMassaMagra = percentualMassaMagra;
        MassaMagraKg = massaMagraKg;
        PercentualAgua = percentualAgua;
        ProtocoloPrega = protocoloPrega;
        BioimpedanciaJson = bioimpedanciaJson;
        CircunferenciasJson = circunferenciasJson;
        PregasJson = pregasJson;
        Observacao = string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim();
        AtualizadoEm = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        Ativo = false;
        AtualizadoEm = DateTime.UtcNow;
    }
}

public sealed class AnexoClinico : AggregateRoot
{
    private AnexoClinico()
    {
    }

    private AnexoClinico(
        Guid empresaId,
        Guid atendimentoClinicoId,
        Guid pacienteId,
        Guid unidadeId,
        Guid funcionarioId,
        TipoAnexoClinico tipo,
        string nome,
        DateTime dataDocumento,
        string? observacao,
        string? categoriaExame,
        string? categoriaDocumento,
        string nomeArquivo,
        string contentType,
        string objectKey,
        long tamanhoBytes)
        : base(Guid.NewGuid())
    {
        EmpresaId = empresaId;
        AtendimentoClinicoId = atendimentoClinicoId;
        PacienteId = pacienteId;
        UnidadeId = unidadeId;
        FuncionarioId = funcionarioId;
        Tipo = tipo;
        Nome = nome;
        DataDocumento = dataDocumento;
        Observacao = observacao;
        CategoriaExame = categoriaExame;
        CategoriaDocumento = categoriaDocumento;
        NomeArquivo = nomeArquivo;
        ContentType = contentType;
        ObjectKey = objectKey;
        TamanhoBytes = tamanhoBytes;
        Ativo = true;
    }

    public Guid EmpresaId { get; private set; }
    public Guid AtendimentoClinicoId { get; private set; }
    public Guid PacienteId { get; private set; }
    public Guid UnidadeId { get; private set; }
    public Guid FuncionarioId { get; private set; }
    public TipoAnexoClinico Tipo { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public DateTime DataDocumento { get; private set; }
    public string? Observacao { get; private set; }
    public string? CategoriaExame { get; private set; }
    public string? CategoriaDocumento { get; private set; }
    public string NomeArquivo { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public string ObjectKey { get; private set; } = string.Empty;
    public long TamanhoBytes { get; private set; }
    public bool Ativo { get; private set; }

    public Empresa Empresa { get; private set; } = null!;
    public AtendimentoClinico AtendimentoClinico { get; private set; } = null!;
    public Paciente Paciente { get; private set; } = null!;
    public Unidade Unidade { get; private set; } = null!;
    public Funcionario Funcionario { get; private set; } = null!;

    public static AnexoClinico Create(
        Guid empresaId,
        Guid atendimentoClinicoId,
        Guid pacienteId,
        Guid unidadeId,
        Guid funcionarioId,
        TipoAnexoClinico tipo,
        string nome,
        DateTime dataDocumento,
        string? observacao,
        string? categoriaExame,
        string? categoriaDocumento,
        string nomeArquivo,
        string contentType,
        string objectKey,
        long tamanhoBytes)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DomainException("Informe o nome do arquivo clínico.");
        }

        if (string.IsNullOrWhiteSpace(objectKey))
        {
            throw new DomainException("Informe o arquivo enviado.");
        }

        return new AnexoClinico(
            empresaId,
            atendimentoClinicoId,
            pacienteId,
            unidadeId,
            funcionarioId,
            tipo,
            nome.Trim(),
            dataDocumento,
            string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim(),
            categoriaExame,
            categoriaDocumento,
            nomeArquivo.Trim(),
            contentType,
            objectKey,
            tamanhoBytes);
    }

    public void UpdateDetails(string nome, DateTime dataDocumento, string? observacao, string? categoriaExame, string? categoriaDocumento)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DomainException("Informe o nome do arquivo clínico.");
        }

        Nome = nome.Trim();
        DataDocumento = dataDocumento;
        Observacao = string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim();
        CategoriaExame = categoriaExame;
        CategoriaDocumento = categoriaDocumento;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        Ativo = false;
        AtualizadoEm = DateTime.UtcNow;
    }
}

public sealed class FotoComparativa : AggregateRoot
{
    private FotoComparativa()
    {
    }

    private FotoComparativa(
        Guid empresaId,
        Guid atendimentoClinicoId,
        Guid pacienteId,
        Guid unidadeId,
        Guid funcionarioId,
        CategoriaFotoComparativa categoria,
        DateTime dataCaptura,
        string? observacao,
        Guid? avaliacaoCorporalId,
        string nomeArquivo,
        string contentType,
        string objectKey,
        long tamanhoBytes)
        : base(Guid.NewGuid())
    {
        EmpresaId = empresaId;
        AtendimentoClinicoId = atendimentoClinicoId;
        PacienteId = pacienteId;
        UnidadeId = unidadeId;
        FuncionarioId = funcionarioId;
        Categoria = categoria;
        DataCaptura = dataCaptura;
        Observacao = observacao;
        AvaliacaoCorporalId = avaliacaoCorporalId;
        NomeArquivo = nomeArquivo;
        ContentType = contentType;
        ObjectKey = objectKey;
        TamanhoBytes = tamanhoBytes;
        Ativo = true;
    }

    public Guid EmpresaId { get; private set; }
    public Guid AtendimentoClinicoId { get; private set; }
    public Guid PacienteId { get; private set; }
    public Guid UnidadeId { get; private set; }
    public Guid FuncionarioId { get; private set; }
    public Guid? AvaliacaoCorporalId { get; private set; }
    public CategoriaFotoComparativa Categoria { get; private set; }
    public DateTime DataCaptura { get; private set; }
    public string? Observacao { get; private set; }
    public string NomeArquivo { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public string ObjectKey { get; private set; } = string.Empty;
    public long TamanhoBytes { get; private set; }
    public bool Ativo { get; private set; }

    public Empresa Empresa { get; private set; } = null!;
    public AtendimentoClinico AtendimentoClinico { get; private set; } = null!;
    public Paciente Paciente { get; private set; } = null!;
    public Unidade Unidade { get; private set; } = null!;
    public Funcionario Funcionario { get; private set; } = null!;
    public AvaliacaoCorporal? AvaliacaoCorporal { get; private set; }

    public static FotoComparativa Create(
        Guid empresaId,
        Guid atendimentoClinicoId,
        Guid pacienteId,
        Guid unidadeId,
        Guid funcionarioId,
        CategoriaFotoComparativa categoria,
        DateTime dataCaptura,
        string? observacao,
        Guid? avaliacaoCorporalId,
        string nomeArquivo,
        string contentType,
        string objectKey,
        long tamanhoBytes)
    {
        if (string.IsNullOrWhiteSpace(objectKey))
        {
            throw new DomainException("Envie a foto comparativa.");
        }

        return new FotoComparativa(
            empresaId,
            atendimentoClinicoId,
            pacienteId,
            unidadeId,
            funcionarioId,
            categoria,
            dataCaptura,
            string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim(),
            avaliacaoCorporalId is null || avaliacaoCorporalId == Guid.Empty ? null : avaliacaoCorporalId,
            nomeArquivo.Trim(),
            contentType,
            objectKey,
            tamanhoBytes);
    }

    public void UpdateDetails(CategoriaFotoComparativa categoria, DateTime dataCaptura, string? observacao)
    {
        Categoria = categoria;
        DataCaptura = dataCaptura;
        Observacao = string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim();
        AtualizadoEm = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        Ativo = false;
        AtualizadoEm = DateTime.UtcNow;
    }
}

public sealed class CalculoEnergeticoRegistro : AggregateRoot
{
    private CalculoEnergeticoRegistro()
    {
    }

    private CalculoEnergeticoRegistro(
        Guid empresaId,
        Guid atendimentoClinicoId,
        Guid pacienteId,
        Guid unidadeId,
        Guid funcionarioId)
        : base(Guid.NewGuid())
    {
        EmpresaId = empresaId;
        AtendimentoClinicoId = atendimentoClinicoId;
        PacienteId = pacienteId;
        UnidadeId = unidadeId;
        FuncionarioId = funcionarioId;
        Ativo = true;
    }

    public Guid EmpresaId { get; private set; }
    public Guid AtendimentoClinicoId { get; private set; }
    public Guid PacienteId { get; private set; }
    public Guid UnidadeId { get; private set; }
    public Guid FuncionarioId { get; private set; }
    public PerfilCalculoEnergetico Perfil { get; private set; }
    public ProtocoloGastoEnergetico Protocolo { get; private set; }
    public NivelAtividadeFisica NivelAtividade { get; private set; }
    public decimal? FatorInjuria { get; private set; }
    public decimal PesoKg { get; private set; }
    public decimal AlturaCm { get; private set; }
    public int Idade { get; private set; }
    public decimal? MassaMagraKg { get; private set; }
    public decimal PesoDesejadoKg { get; private set; }
    public int TempoDias { get; private set; }
    public string? AtividadesJson { get; private set; }
    public decimal GastoEnergeticoBasal { get; private set; }
    public decimal GastoEnergeticoTotal { get; private set; }
    public decimal AjusteCaloricoDiario { get; private set; }
    public decimal MetaCaloricaDiaria { get; private set; }
    public bool Ativo { get; private set; }

    public Empresa Empresa { get; private set; } = null!;
    public AtendimentoClinico AtendimentoClinico { get; private set; } = null!;
    public Paciente Paciente { get; private set; } = null!;
    public Unidade Unidade { get; private set; } = null!;
    public Funcionario Funcionario { get; private set; } = null!;

    public static CalculoEnergeticoRegistro Create(
        Guid empresaId,
        Guid atendimentoClinicoId,
        Guid pacienteId,
        Guid unidadeId,
        Guid funcionarioId)
    {
        return new CalculoEnergeticoRegistro(empresaId, atendimentoClinicoId, pacienteId, unidadeId, funcionarioId);
    }

    public void ApplyResult(
        PerfilCalculoEnergetico perfil,
        ProtocoloGastoEnergetico protocolo,
        NivelAtividadeFisica nivelAtividade,
        decimal? fatorInjuria,
        decimal pesoKg,
        decimal alturaCm,
        int idade,
        decimal? massaMagraKg,
        decimal pesoDesejadoKg,
        int tempoDias,
        string? atividadesJson,
        decimal gastoEnergeticoBasal,
        decimal gastoEnergeticoTotal,
        decimal ajusteCaloricoDiario,
        decimal metaCaloricaDiaria)
    {
        Perfil = perfil;
        Protocolo = protocolo;
        NivelAtividade = nivelAtividade;
        FatorInjuria = fatorInjuria;
        PesoKg = pesoKg;
        AlturaCm = alturaCm;
        Idade = idade;
        MassaMagraKg = massaMagraKg;
        PesoDesejadoKg = pesoDesejadoKg;
        TempoDias = tempoDias;
        AtividadesJson = atividadesJson;
        GastoEnergeticoBasal = gastoEnergeticoBasal;
        GastoEnergeticoTotal = gastoEnergeticoTotal;
        AjusteCaloricoDiario = ajusteCaloricoDiario;
        MetaCaloricaDiaria = metaCaloricaDiaria;
        AtualizadoEm = DateTime.UtcNow;
    }
}

public sealed class RegraBolsoRegistro : AggregateRoot
{
    private RegraBolsoRegistro()
    {
    }

    private RegraBolsoRegistro(
        Guid empresaId,
        Guid atendimentoClinicoId,
        Guid pacienteId,
        Guid unidadeId,
        Guid funcionarioId)
        : base(Guid.NewGuid())
    {
        EmpresaId = empresaId;
        AtendimentoClinicoId = atendimentoClinicoId;
        PacienteId = pacienteId;
        UnidadeId = unidadeId;
        FuncionarioId = funcionarioId;
        Ativo = true;
    }

    public Guid EmpresaId { get; private set; }
    public Guid AtendimentoClinicoId { get; private set; }
    public Guid PacienteId { get; private set; }
    public Guid UnidadeId { get; private set; }
    public Guid FuncionarioId { get; private set; }
    public ObjetivoRegraBolso Objetivo { get; private set; }
    public decimal PesoKg { get; private set; }
    public decimal GastoEnergeticoTotal { get; private set; }
    public decimal Calorias { get; private set; }
    public decimal ProteinasG { get; private set; }
    public decimal CarboidratosG { get; private set; }
    public decimal GordurasG { get; private set; }
    public bool Ativo { get; private set; }

    public Empresa Empresa { get; private set; } = null!;
    public AtendimentoClinico AtendimentoClinico { get; private set; } = null!;
    public Paciente Paciente { get; private set; } = null!;
    public Unidade Unidade { get; private set; } = null!;
    public Funcionario Funcionario { get; private set; } = null!;

    public static RegraBolsoRegistro Create(
        Guid empresaId,
        Guid atendimentoClinicoId,
        Guid pacienteId,
        Guid unidadeId,
        Guid funcionarioId)
    {
        return new RegraBolsoRegistro(empresaId, atendimentoClinicoId, pacienteId, unidadeId, funcionarioId);
    }

    public void ApplyResult(
        ObjetivoRegraBolso objetivo,
        decimal pesoKg,
        decimal gastoEnergeticoTotal,
        decimal calorias,
        decimal proteinasG,
        decimal carboidratosG,
        decimal gordurasG)
    {
        Objetivo = objetivo;
        PesoKg = pesoKg;
        GastoEnergeticoTotal = gastoEnergeticoTotal;
        Calorias = calorias;
        ProteinasG = proteinasG;
        CarboidratosG = carboidratosG;
        GordurasG = gordurasG;
        AtualizadoEm = DateTime.UtcNow;
    }
}
