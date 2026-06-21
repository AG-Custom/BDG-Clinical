using BGD.CLINICAL.Domain.Common;
using BGD.CLINICAL.Domain.Enums;

namespace BGD.CLINICAL.Domain.Entities;

public sealed class Agendamento : AggregateRoot
{
    private Agendamento()
    {
    }

    public Agendamento(
        Guid empresaId,
        Guid unidadeId,
        Guid pacienteId,
        Guid funcionarioId,
        Guid? compraPacienteId,
        TipoAgendamento tipo,
        DateTime dataInicio,
        DateTime dataFim,
        string titulo,
        Guid criadoPorId)
        : base(Guid.NewGuid())
    {
        EmpresaId = empresaId;
        UnidadeId = unidadeId;
        PacienteId = pacienteId;
        FuncionarioId = funcionarioId;
        CompraPacienteId = compraPacienteId;
        Tipo = tipo;
        Status = StatusAgendamento.Agendado;
        DataInicio = dataInicio;
        DataFim = dataFim;
        Titulo = titulo;
        CriadoPorId = criadoPorId;
    }

    public Guid EmpresaId { get; private set; }
    public Guid UnidadeId { get; private set; }
    public Guid PacienteId { get; private set; }
    public Guid FuncionarioId { get; private set; }
    public Guid? CompraPacienteId { get; private set; }
    public TipoAgendamento Tipo { get; private set; }
    public StatusAgendamento Status { get; private set; }
    public DateTime DataInicio { get; private set; }
    public DateTime DataFim { get; private set; }
    public string Titulo { get; private set; } = string.Empty;
    public string? Observacao { get; private set; }
    public Guid CriadoPorId { get; private set; }
    public Guid? CanceladoPorId { get; private set; }
    public string? MotivoCancelamento { get; private set; }

    public Empresa Empresa { get; private set; } = null!;
    public Unidade Unidade { get; private set; } = null!;
    public Paciente Paciente { get; private set; } = null!;
    public Funcionario Funcionario { get; private set; } = null!;
    public CompraPaciente? CompraPaciente { get; private set; }
    public Usuario CriadoPor { get; private set; } = null!;
    public Usuario? CanceladoPor { get; private set; }
    public AplicacaoPaciente? AplicacaoPaciente { get; private set; }
    public AgendamentoGoogleSync? GoogleSync { get; private set; }
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
