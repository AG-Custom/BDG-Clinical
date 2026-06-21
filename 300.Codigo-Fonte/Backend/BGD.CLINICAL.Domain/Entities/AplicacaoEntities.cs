using BGD.CLINICAL.Domain.Common;

namespace BGD.CLINICAL.Domain.Entities;

public sealed class AplicacaoPaciente : AggregateRoot
{
    private AplicacaoPaciente()
    {
    }

    public AplicacaoPaciente(Guid empresaId, Guid pacienteId, Guid compraPacienteId, Guid produtoId, Guid funcionarioId, Guid unidadeId, DateTime dataAplicacao, decimal quantidadeUtilizada, Guid? agendamentoId = null)
        : base(Guid.NewGuid())
    {
        EmpresaId = empresaId;
        PacienteId = pacienteId;
        CompraPacienteId = compraPacienteId;
        ProdutoId = produtoId;
        FuncionarioId = funcionarioId;
        UnidadeId = unidadeId;
        AgendamentoId = agendamentoId;
        DataAplicacao = dataAplicacao;
        QuantidadeUtilizada = quantidadeUtilizada;
    }

    public Guid EmpresaId { get; private set; }
    public Guid PacienteId { get; private set; }
    public Guid CompraPacienteId { get; private set; }
    public Guid ProdutoId { get; private set; }
    public Guid FuncionarioId { get; private set; }
    public Guid UnidadeId { get; private set; }
    public Guid? AgendamentoId { get; private set; }
    public DateTime DataAplicacao { get; private set; }
    public decimal QuantidadeUtilizada { get; private set; }
    public decimal? Peso { get; private set; }
    public string? Observacao { get; private set; }
    public bool Realizado { get; private set; }

    public Empresa Empresa { get; private set; } = null!;
    public Paciente Paciente { get; private set; } = null!;
    public CompraPaciente CompraPaciente { get; private set; } = null!;
    public Produto Produto { get; private set; } = null!;
    public Funcionario Funcionario { get; private set; } = null!;
    public Unidade Unidade { get; private set; } = null!;
    public Agendamento? Agendamento { get; private set; }
    public ICollection<AplicacaoSintoma> Sintomas { get; private set; } = [];
    public ICollection<MovimentacaoEstoque> MovimentacoesEstoque { get; private set; } = [];
}

public sealed class AplicacaoSintoma : AggregateRoot
{
    private AplicacaoSintoma()
    {
    }

    public AplicacaoSintoma(Guid aplicacaoPacienteId, Guid sintomaId)
        : base(Guid.NewGuid())
    {
        AplicacaoPacienteId = aplicacaoPacienteId;
        SintomaId = sintomaId;
    }

    public Guid AplicacaoPacienteId { get; private set; }
    public Guid SintomaId { get; private set; }

    public AplicacaoPaciente AplicacaoPaciente { get; private set; } = null!;
    public Sintoma Sintoma { get; private set; } = null!;
}
