using BGD.CLINICAL.Domain.Common;
using BGD.CLINICAL.Domain.Exceptions;

namespace BGD.CLINICAL.Domain.Entities;

public sealed class AplicacaoPaciente : AggregateRoot
{
    private AplicacaoPaciente()
    {
    }

    private AplicacaoPaciente(
        Guid empresaId,
        Guid pacienteId,
        Guid? compraPacienteId,
        Guid? produtoId,
        Guid? procedimentoId,
        Guid funcionarioId,
        Guid unidadeId,
        DateTime dataAplicacao,
        decimal? quantidadeUtilizada,
        decimal? peso,
        string? observacao,
        Guid? agendamentoId)
        : base(Guid.NewGuid())
    {
        EmpresaId = empresaId;
        PacienteId = pacienteId;
        CompraPacienteId = compraPacienteId;
        ProdutoId = produtoId;
        ProcedimentoId = procedimentoId;
        FuncionarioId = funcionarioId;
        UnidadeId = unidadeId;
        AgendamentoId = agendamentoId;
        DataAplicacao = dataAplicacao;
        QuantidadeUtilizada = quantidadeUtilizada;
        Peso = peso;
        Observacao = observacao;
        Realizado = true;
        Cancelada = false;
    }

    public Guid EmpresaId { get; private set; }
    public Guid PacienteId { get; private set; }
    public Guid? CompraPacienteId { get; private set; }
    public Guid? ProdutoId { get; private set; }
    public Guid? ProcedimentoId { get; private set; }
    public Guid FuncionarioId { get; private set; }
    public Guid UnidadeId { get; private set; }
    public Guid? AgendamentoId { get; private set; }
    public DateTime DataAplicacao { get; private set; }
    public decimal? QuantidadeUtilizada { get; private set; }
    public decimal? Peso { get; private set; }
    public string? Observacao { get; private set; }
    public bool Realizado { get; private set; }
    public bool Cancelada { get; private set; }

    public Empresa Empresa { get; private set; } = null!;
    public Paciente Paciente { get; private set; } = null!;
    public CompraPaciente? CompraPaciente { get; private set; }
    public Produto? Produto { get; private set; }
    public Procedimento? Procedimento { get; private set; }
    public Funcionario Funcionario { get; private set; } = null!;
    public Unidade Unidade { get; private set; } = null!;
    public Agendamento? Agendamento { get; private set; }
    public ICollection<AplicacaoSintoma> Sintomas { get; private set; } = [];
    public ICollection<MovimentacaoEstoque> MovimentacoesEstoque { get; private set; } = [];

    public static AplicacaoPaciente CreateRealizada(
        Guid empresaId,
        Guid pacienteId,
        Guid? compraPacienteId,
        Guid? produtoId,
        Guid? procedimentoId,
        Guid funcionarioId,
        Guid unidadeId,
        DateTime dataAplicacao,
        decimal? quantidadeUtilizada,
        decimal? peso = null,
        string? observacao = null,
        Guid? agendamentoId = null)
    {
        if (empresaId == Guid.Empty)
        {
            throw new DomainException("Informe a empresa da aplicação.");
        }

        if (pacienteId == Guid.Empty)
        {
            throw new DomainException("Informe o paciente da aplicação.");
        }

        if (!procedimentoId.HasValue || procedimentoId.Value == Guid.Empty)
        {
            throw new DomainException("Informe o procedimento da aplicação.");
        }

        if (produtoId.HasValue && produtoId.Value != Guid.Empty && quantidadeUtilizada is null or <= 0)
        {
            throw new DomainException("A quantidade utilizada deve ser maior que zero.");
        }

        if (!produtoId.HasValue && quantidadeUtilizada.HasValue)
        {
            throw new DomainException("Quantidade utilizada não se aplica a procedimentos sem produto aplicado.");
        }

        if (funcionarioId == Guid.Empty)
        {
            throw new DomainException("Informe o aplicador da aplicação.");
        }

        if (unidadeId == Guid.Empty)
        {
            throw new DomainException("Informe a unidade da aplicação.");
        }

        if (peso.HasValue && peso.Value <= 0)
        {
            throw new DomainException("O peso deve ser maior que zero quando informado.");
        }

        if (!string.IsNullOrWhiteSpace(observacao) && observacao.Length > 2000)
        {
            throw new DomainException("A observação deve ter no máximo 2000 caracteres.");
        }

        return new AplicacaoPaciente(
            empresaId,
            pacienteId,
            compraPacienteId,
            produtoId,
            procedimentoId,
            funcionarioId,
            unidadeId,
            dataAplicacao,
            quantidadeUtilizada,
            peso,
            string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim(),
            agendamentoId);
    }

    public void UpdateDetails(decimal? peso, string? observacao, DateTime dataAplicacao)
    {
        if (Cancelada)
        {
            throw new DomainException("Não é possível editar uma aplicação cancelada.");
        }

        if (peso.HasValue && peso.Value <= 0)
        {
            throw new DomainException("O peso deve ser maior que zero quando informado.");
        }

        if (!string.IsNullOrWhiteSpace(observacao) && observacao.Length > 2000)
        {
            throw new DomainException("A observação deve ter no máximo 2000 caracteres.");
        }

        Peso = peso;
        Observacao = string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim();
        DataAplicacao = dataAplicacao;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (!Realizado)
        {
            throw new DomainException("Somente aplicações realizadas podem ser canceladas.");
        }

        if (Cancelada)
        {
            throw new DomainException("Esta aplicação já foi cancelada.");
        }

        Cancelada = true;
        AtualizadoEm = DateTime.UtcNow;
    }
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
