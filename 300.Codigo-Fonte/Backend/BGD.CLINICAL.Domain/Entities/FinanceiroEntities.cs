using BGD.CLINICAL.Domain.Common;
using BGD.CLINICAL.Domain.Enums;

namespace BGD.CLINICAL.Domain.Entities;

public sealed class FormaPagamento : AggregateRoot
{
    private FormaPagamento()
    {
    }

    public FormaPagamento(Guid empresaId, string nome)
        : base(Guid.NewGuid())
    {
        EmpresaId = empresaId;
        Nome = nome;
        Ativo = true;
    }

    public Guid EmpresaId { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public bool Ativo { get; private set; }

    public Empresa Empresa { get; private set; } = null!;
}

public sealed class ContaReceber : AggregateRoot
{
    private ContaReceber()
    {
    }

    public ContaReceber(Guid empresaId, Guid pacienteId, Guid? compraPacienteId, decimal valor, DateOnly dataVencimento)
        : base(Guid.NewGuid())
    {
        EmpresaId = empresaId;
        PacienteId = pacienteId;
        CompraPacienteId = compraPacienteId;
        Valor = valor;
        DataVencimento = dataVencimento;
        Status = StatusContaReceber.Aberta;
    }

    public Guid EmpresaId { get; private set; }
    public Guid PacienteId { get; private set; }
    public Guid? CompraPacienteId { get; private set; }
    public decimal Valor { get; private set; }
    public DateOnly DataVencimento { get; private set; }
    public StatusContaReceber Status { get; private set; }

    public Empresa Empresa { get; private set; } = null!;
    public Paciente Paciente { get; private set; } = null!;
    public CompraPaciente? CompraPaciente { get; private set; }
    public ICollection<PagamentoPaciente> Pagamentos { get; private set; } = [];
}

public sealed class PagamentoPaciente : AggregateRoot
{
    private PagamentoPaciente()
    {
    }

    public PagamentoPaciente(Guid empresaId, Guid pacienteId, Guid contaReceberId, Guid formaPagamentoId, decimal valor, DateTime dataPagamento)
        : base(Guid.NewGuid())
    {
        EmpresaId = empresaId;
        PacienteId = pacienteId;
        ContaReceberId = contaReceberId;
        FormaPagamentoId = formaPagamentoId;
        Valor = valor;
        DataPagamento = dataPagamento;
    }

    public Guid EmpresaId { get; private set; }
    public Guid PacienteId { get; private set; }
    public Guid ContaReceberId { get; private set; }
    public Guid FormaPagamentoId { get; private set; }
    public decimal Valor { get; private set; }
    public DateTime DataPagamento { get; private set; }
    public string? Observacoes { get; private set; }

    public Empresa Empresa { get; private set; } = null!;
    public Paciente Paciente { get; private set; } = null!;
    public ContaReceber ContaReceber { get; private set; } = null!;
    public FormaPagamento FormaPagamento { get; private set; } = null!;
}
