using BGD.CLINICAL.Domain.Common;
using BGD.CLINICAL.Domain.Enums;
using BGD.CLINICAL.Domain.Exceptions;

namespace BGD.CLINICAL.Domain.Entities;

public sealed class Agendamento : AggregateRoot
{
    private Agendamento()
    {
    }

    public Guid EmpresaId { get; private set; }
    public Guid UnidadeId { get; private set; }
    public Guid PacienteId { get; private set; }
    public Guid FuncionarioId { get; private set; }
    public Guid? ProcedimentoId { get; private set; }
    public TipoAgendamento Tipo { get; private set; }
    public StatusAgendamento Status { get; private set; }
    public DateTime DataInicio { get; private set; }
    public DateTime DataFim { get; private set; }
    public string? Observacao { get; private set; }
    public bool ExcecaoHorario { get; private set; }
    public Guid CriadoPorId { get; private set; }
    public Guid? CanceladoPorId { get; private set; }
    public string? MotivoCancelamento { get; private set; }

    public Empresa Empresa { get; private set; } = null!;
    public Unidade Unidade { get; private set; } = null!;
    public Paciente Paciente { get; private set; } = null!;
    public Funcionario Funcionario { get; private set; } = null!;
    public Procedimento? Procedimento { get; private set; }
    public Usuario CriadoPor { get; private set; } = null!;
    public Usuario? CanceladoPor { get; private set; }
    public AplicacaoPaciente? AplicacaoPaciente { get; private set; }

    public static Agendamento Create(
        Guid empresaId,
        Guid unidadeId,
        Guid pacienteId,
        Guid funcionarioId,
        Guid? procedimentoId,
        TipoAgendamento tipo,
        DateTime dataInicio,
        DateTime dataFim,
        string? observacao,
        bool excecaoHorario,
        Guid criadoPorId)
    {
        ValidateCore(
            empresaId,
            unidadeId,
            pacienteId,
            funcionarioId,
            tipo,
            procedimentoId,
            dataInicio,
            dataFim,
            criadoPorId);

        return new Agendamento
        {
            Id = Guid.NewGuid(),
            CriadoEm = DateTime.UtcNow,
            EmpresaId = empresaId,
            UnidadeId = unidadeId,
            PacienteId = pacienteId,
            FuncionarioId = funcionarioId,
            ProcedimentoId = procedimentoId,
            Tipo = tipo,
            Status = StatusAgendamento.Agendado,
            DataInicio = dataInicio,
            DataFim = dataFim,
            Observacao = string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim(),
            ExcecaoHorario = excecaoHorario,
            CriadoPorId = criadoPorId
        };
    }

    public void UpdateDetails(
        Guid unidadeId,
        Guid pacienteId,
        Guid funcionarioId,
        Guid? procedimentoId,
        TipoAgendamento tipo,
        DateTime dataInicio,
        DateTime dataFim,
        string? observacao,
        bool excecaoHorario)
    {
        EnsureEditable();

        ValidateCore(
            EmpresaId,
            unidadeId,
            pacienteId,
            funcionarioId,
            tipo,
            procedimentoId,
            dataInicio,
            dataFim,
            CriadoPorId);

        UnidadeId = unidadeId;
        PacienteId = pacienteId;
        FuncionarioId = funcionarioId;
        ProcedimentoId = procedimentoId;
        Tipo = tipo;
        DataInicio = dataInicio;
        DataFim = dataFim;
        Observacao = string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim();
        ExcecaoHorario = excecaoHorario;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void Confirm()
    {
        if (Status != StatusAgendamento.Agendado)
        {
            throw new DomainException("Somente agendamentos pendentes podem ser confirmados.");
        }

        Status = StatusAgendamento.Confirmado;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void MarkAsCompleted()
    {
        if (Status is StatusAgendamento.Cancelado or StatusAgendamento.Faltou)
        {
            throw new DomainException("Agendamento cancelado ou com falta não pode ser concluído.");
        }

        if (Status == StatusAgendamento.Concluido)
        {
            throw new DomainException("Agendamento já está concluído.");
        }

        if (Status != StatusAgendamento.Confirmado && Status != StatusAgendamento.Agendado)
        {
            throw new DomainException("Somente agendamentos confirmados ou pendentes podem ser concluídos.");
        }

        Status = StatusAgendamento.Concluido;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void MarkAsNoShow()
    {
        if (Status is StatusAgendamento.Cancelado or StatusAgendamento.Faltou or StatusAgendamento.Concluido)
        {
            throw new DomainException("Este agendamento não pode ser marcado como falta.");
        }

        Status = StatusAgendamento.Faltou;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void Cancel(Guid canceladoPorId, string motivo)
    {
        if (Status == StatusAgendamento.Cancelado)
        {
            throw new DomainException("Agendamento já está cancelado.");
        }

        if (Status == StatusAgendamento.Concluido)
        {
            throw new DomainException("Agendamento concluído não pode ser cancelado.");
        }

        if (canceladoPorId == Guid.Empty)
        {
            throw new DomainException("Informe o usuário que cancela o agendamento.");
        }

        if (string.IsNullOrWhiteSpace(motivo))
        {
            throw new DomainException("Informe o motivo do cancelamento.");
        }

        Status = StatusAgendamento.Cancelado;
        CanceladoPorId = canceladoPorId;
        MotivoCancelamento = motivo.Trim();
        AtualizadoEm = DateTime.UtcNow;
    }

    private void EnsureEditable()
    {
        if (Status is not (StatusAgendamento.Agendado or StatusAgendamento.Confirmado))
        {
            throw new DomainException("Somente agendamentos pendentes ou confirmados podem ser alterados.");
        }
    }

    private static void ValidateCore(
        Guid empresaId,
        Guid unidadeId,
        Guid pacienteId,
        Guid funcionarioId,
        TipoAgendamento tipo,
        Guid? procedimentoId,
        DateTime dataInicio,
        DateTime dataFim,
        Guid criadoPorId)
    {
        if (empresaId == Guid.Empty)
        {
            throw new DomainException("Informe a empresa do agendamento.");
        }

        if (unidadeId == Guid.Empty)
        {
            throw new DomainException("Informe a unidade do agendamento.");
        }

        if (pacienteId == Guid.Empty)
        {
            throw new DomainException("Informe o paciente do agendamento.");
        }

        if (funcionarioId == Guid.Empty)
        {
            throw new DomainException("Informe o funcionário do agendamento.");
        }

        if (criadoPorId == Guid.Empty)
        {
            throw new DomainException("Informe o usuário que cria o agendamento.");
        }

        if (dataFim <= dataInicio)
        {
            throw new DomainException("A data de término deve ser posterior à data de início.");
        }

        if (tipo == TipoAgendamento.Aplicacao && (!procedimentoId.HasValue || procedimentoId.Value == Guid.Empty))
        {
            throw new DomainException("Informe o procedimento para agendamentos do tipo aplicação.");
        }

        if (tipo != TipoAgendamento.Aplicacao && procedimentoId.HasValue && procedimentoId.Value != Guid.Empty)
        {
            throw new DomainException("Procedimento só pode ser informado em agendamentos do tipo aplicação.");
        }
    }
}

public sealed class HorarioFuncionamentoUnidade : AggregateRoot
{
    private HorarioFuncionamentoUnidade()
    {
    }

    public HorarioFuncionamentoUnidade(
        Guid empresaId,
        Guid unidadeId,
        DiaSemana diaSemana,
        TimeOnly horaInicio,
        TimeOnly horaFim)
        : base(Guid.NewGuid())
    {
        ValidateTimes(horaInicio, horaFim);

        EmpresaId = empresaId;
        UnidadeId = unidadeId;
        DiaSemana = diaSemana;
        HoraInicio = horaInicio;
        HoraFim = horaFim;
        Ativo = true;
        CriadoEm = DateTime.UtcNow;
    }

    public Guid EmpresaId { get; private set; }
    public Guid UnidadeId { get; private set; }
    public DiaSemana DiaSemana { get; private set; }
    public TimeOnly HoraInicio { get; private set; }
    public TimeOnly HoraFim { get; private set; }
    public bool Ativo { get; private set; }

    public Empresa Empresa { get; private set; } = null!;
    public Unidade Unidade { get; private set; } = null!;

    public void UpdateDetails(DiaSemana diaSemana, TimeOnly horaInicio, TimeOnly horaFim, bool ativo)
    {
        ValidateTimes(horaInicio, horaFim);

        DiaSemana = diaSemana;
        HoraInicio = horaInicio;
        HoraFim = horaFim;
        Ativo = ativo;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void SetActive(bool ativo)
    {
        if (Ativo == ativo)
        {
            throw new DomainException(ativo
                ? "Horário de funcionamento já está ativo."
                : "Horário de funcionamento já está inativo.");
        }

        Ativo = ativo;
        AtualizadoEm = DateTime.UtcNow;
    }

    private static void ValidateTimes(TimeOnly horaInicio, TimeOnly horaFim)
    {
        if (horaFim <= horaInicio)
        {
            throw new DomainException("O horário de término deve ser posterior ao horário de início.");
        }
    }
}

public sealed class DisponibilidadeFuncionario : AggregateRoot
{
    private DisponibilidadeFuncionario()
    {
    }

    public DisponibilidadeFuncionario(
        Guid empresaId,
        Guid funcionarioId,
        Guid unidadeId,
        DiaSemana diaSemana,
        TimeOnly horaInicio,
        TimeOnly horaFim)
        : base(Guid.NewGuid())
    {
        EmpresaId = empresaId;
        FuncionarioId = funcionarioId;
        UnidadeId = unidadeId;
        DiaSemana = diaSemana;
        HoraInicio = horaInicio;
        HoraFim = horaFim;
        Ativo = true;
    }

    public Guid EmpresaId { get; private set; }
    public Guid FuncionarioId { get; private set; }
    public Guid UnidadeId { get; private set; }
    public DiaSemana DiaSemana { get; private set; }
    public TimeOnly HoraInicio { get; private set; }
    public TimeOnly HoraFim { get; private set; }
    public bool Ativo { get; private set; }

    public Empresa Empresa { get; private set; } = null!;
    public Funcionario Funcionario { get; private set; } = null!;
    public Unidade Unidade { get; private set; } = null!;
}

public sealed class BloqueioAgenda : AggregateRoot
{
    private BloqueioAgenda()
    {
    }

    public BloqueioAgenda(
        Guid empresaId,
        Guid funcionarioId,
        Guid unidadeId,
        DateTime dataInicio,
        DateTime dataFim,
        string motivo,
        Guid criadoPorId)
        : base(Guid.NewGuid())
    {
        EmpresaId = empresaId;
        FuncionarioId = funcionarioId;
        UnidadeId = unidadeId;
        DataInicio = dataInicio;
        DataFim = dataFim;
        Motivo = motivo;
        CriadoPorId = criadoPorId;
    }

    public Guid EmpresaId { get; private set; }
    public Guid FuncionarioId { get; private set; }
    public Guid UnidadeId { get; private set; }
    public DateTime DataInicio { get; private set; }
    public DateTime DataFim { get; private set; }
    public string Motivo { get; private set; } = string.Empty;
    public Guid CriadoPorId { get; private set; }

    public Empresa Empresa { get; private set; } = null!;
    public Funcionario Funcionario { get; private set; } = null!;
    public Unidade Unidade { get; private set; } = null!;
    public Usuario CriadoPor { get; private set; } = null!;
}

public sealed class ContaGoogleConectada : AggregateRoot
{
    private ContaGoogleConectada()
    {
    }

    public ContaGoogleConectada(
        Guid empresaId,
        Guid usuarioId,
        Guid funcionarioId,
        string googleEmail,
        string googleAccountId,
        string accessToken,
        string refreshToken,
        string escoposAutorizados)
        : base(Guid.NewGuid())
    {
        EmpresaId = empresaId;
        UsuarioId = usuarioId;
        FuncionarioId = funcionarioId;
        GoogleEmail = googleEmail;
        GoogleAccountId = googleAccountId;
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        EscoposAutorizados = escoposAutorizados;
        Ativo = true;
        ConectadoEm = DateTime.UtcNow;
    }

    public Guid EmpresaId { get; private set; }
    public Guid UsuarioId { get; private set; }
    public Guid FuncionarioId { get; private set; }
    public string GoogleEmail { get; private set; } = string.Empty;
    public string GoogleAccountId { get; private set; } = string.Empty;
    public string AccessToken { get; private set; } = string.Empty;
    public string RefreshToken { get; private set; } = string.Empty;
    public string EscoposAutorizados { get; private set; } = string.Empty;
    public bool Ativo { get; private set; }
    public DateTime ConectadoEm { get; private set; }

    public Empresa Empresa { get; private set; } = null!;
    public Usuario Usuario { get; private set; } = null!;
    public Funcionario Funcionario { get; private set; } = null!;
    public ICollection<AgendaGoogle> Agendas { get; private set; } = [];
}

public sealed class AgendaGoogle : AggregateRoot
{
    private AgendaGoogle()
    {
    }

    public AgendaGoogle(
        Guid empresaId,
        Guid contaGoogleConectadaId,
        string googleCalendarId,
        string nome,
        bool principal)
        : base(Guid.NewGuid())
    {
        EmpresaId = empresaId;
        ContaGoogleConectadaId = contaGoogleConectadaId;
        GoogleCalendarId = googleCalendarId;
        Nome = nome;
        Principal = principal;
        Ativo = true;
    }

    public Guid EmpresaId { get; private set; }
    public Guid ContaGoogleConectadaId { get; private set; }
    public string GoogleCalendarId { get; private set; } = string.Empty;
    public string Nome { get; private set; } = string.Empty;
    public bool Principal { get; private set; }
    public bool Ativo { get; private set; }

    public Empresa Empresa { get; private set; } = null!;
    public ContaGoogleConectada ContaGoogleConectada { get; private set; } = null!;
    public ICollection<AgendamentoGoogleSync> Sincronizacoes { get; private set; } = [];
}

public sealed class AgendamentoGoogleSync : AggregateRoot
{
    private AgendamentoGoogleSync()
    {
    }

    public AgendamentoGoogleSync(
        Guid empresaId,
        Guid agendamentoId,
        Guid agendaGoogleId,
        string? googleEventId)
        : base(Guid.NewGuid())
    {
        EmpresaId = empresaId;
        AgendamentoId = agendamentoId;
        AgendaGoogleId = agendaGoogleId;
        GoogleEventId = googleEventId;
        StatusSync = StatusSyncGoogle.Pendente;
    }

    public Guid EmpresaId { get; private set; }
    public Guid AgendamentoId { get; private set; }
    public Guid AgendaGoogleId { get; private set; }
    public string? GoogleEventId { get; private set; }
    public StatusSyncGoogle StatusSync { get; private set; }
    public DateTime? UltimaSincronizacao { get; private set; }
    public string? ErroSync { get; private set; }

    public Empresa Empresa { get; private set; } = null!;
    public Agendamento Agendamento { get; private set; } = null!;
    public AgendaGoogle AgendaGoogle { get; private set; } = null!;
}
