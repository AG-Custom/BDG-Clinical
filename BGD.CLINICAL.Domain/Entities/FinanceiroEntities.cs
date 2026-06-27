using BGD.CLINICAL.Domain.Common;
using BGD.CLINICAL.Domain.Enums;
using BGD.CLINICAL.Domain.Exceptions;

namespace BGD.CLINICAL.Domain.Entities;

public sealed class FormaPagamento : AggregateRoot
{
    private FormaPagamento()
    {
    }

    public Guid EmpresaId { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string? Tipo { get; private set; }
    public bool Ativo { get; private set; }

    public Empresa Empresa { get; private set; } = null!;

    public static FormaPagamento Create(Guid empresaId, string nome, string? tipo)
    {
        if (empresaId == Guid.Empty)
        {
            throw new DomainException("Informe a empresa da forma de pagamento.");
        }

        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DomainException("Informe o nome da forma de pagamento.");
        }

        return new FormaPagamento
        {
            Id = Guid.NewGuid(),
            CriadoEm = DateTime.UtcNow,
            EmpresaId = empresaId,
            Nome = nome.Trim(),
            Tipo = string.IsNullOrWhiteSpace(tipo) ? null : tipo.Trim(),
            Ativo = true
        };
    }

    public void Deactivate()
    {
        if (!Ativo)
        {
            throw new DomainException("Forma de pagamento já está inativa.");
        }

        Ativo = false;
        AtualizadoEm = DateTime.UtcNow;
    }
}

public sealed class ContaReceber : AggregateRoot
{
    private ContaReceber()
    {
    }

    public Guid EmpresaId { get; private set; }
    public Guid PacienteId { get; private set; }
    public Guid? CompraPacienteId { get; private set; }
    public DateOnly DataVencimento { get; private set; }
    public decimal ValorTotal { get; private set; }
    public StatusContaReceber Status { get; private set; }
    public string? Observacao { get; private set; }

    public Empresa Empresa { get; private set; } = null!;
    public Paciente Paciente { get; private set; } = null!;
    public CompraPaciente? CompraPaciente { get; private set; }
    public ICollection<PagamentoPaciente> Pagamentos { get; private set; } = [];

    public static ContaReceber Create(
        Guid empresaId,
        Guid pacienteId,
        Guid? compraPacienteId,
        DateOnly dataVencimento,
        decimal valorTotal)
    {
        if (empresaId == Guid.Empty || pacienteId == Guid.Empty)
        {
            throw new DomainException("Informe empresa e paciente da conta a receber.");
        }

        if (valorTotal <= 0)
        {
            throw new DomainException("O valor total deve ser maior que zero.");
        }

        return new ContaReceber
        {
            Id = Guid.NewGuid(),
            CriadoEm = DateTime.UtcNow,
            EmpresaId = empresaId,
            PacienteId = pacienteId,
            CompraPacienteId = compraPacienteId,
            DataVencimento = dataVencimento,
            ValorTotal = valorTotal,
            Status = StatusContaReceber.Aberto
        };
    }

    public void RegisterPayment(decimal valorPago)
    {
        if (Status == StatusContaReceber.Cancelado)
        {
            throw new DomainException("Conta cancelada não aceita pagamento.");
        }

        if (valorPago <= 0)
        {
            throw new DomainException("O valor pago deve ser maior que zero.");
        }

        var totalPago = Pagamentos.Sum(p => p.ValorPago) + valorPago;

        if (totalPago > ValorTotal)
        {
            throw new DomainException("O valor pago excede o total da conta.");
        }

        Status = totalPago >= ValorTotal
            ? StatusContaReceber.Pago
            : StatusContaReceber.Aberto;

        AtualizadoEm = DateTime.UtcNow;
    }

    public void Cancel(string? observacao)
    {
        if (Status == StatusContaReceber.Pago)
        {
            throw new DomainException("Conta paga não pode ser cancelada.");
        }

        if (Status == StatusContaReceber.Cancelado)
        {
            throw new DomainException("Conta já está cancelada.");
        }

        Status = StatusContaReceber.Cancelado;
        Observacao = string.IsNullOrWhiteSpace(observacao) ? Observacao : observacao.Trim();
        AtualizadoEm = DateTime.UtcNow;
    }
}

public sealed class PagamentoPaciente : AggregateRoot
{
    private PagamentoPaciente()
    {
    }

    public Guid EmpresaId { get; private set; }
    public Guid PacienteId { get; private set; }
    public Guid ContaReceberId { get; private set; }
    public Guid FormaPagamentoId { get; private set; }
    public DateTime DataPagamento { get; private set; }
    public decimal ValorPago { get; private set; }
    public string? Observacao { get; private set; }

    public Empresa Empresa { get; private set; } = null!;
    public Paciente Paciente { get; private set; } = null!;
    public ContaReceber ContaReceber { get; private set; } = null!;
    public FormaPagamento FormaPagamento { get; private set; } = null!;

    public static PagamentoPaciente Create(
        Guid empresaId,
        Guid pacienteId,
        Guid contaReceberId,
        Guid formaPagamentoId,
        DateTime dataPagamento,
        decimal valorPago)
    {
        if (empresaId == Guid.Empty || pacienteId == Guid.Empty || contaReceberId == Guid.Empty || formaPagamentoId == Guid.Empty)
        {
            throw new DomainException("Informe todos os vínculos do pagamento.");
        }

        if (valorPago <= 0)
        {
            throw new DomainException("O valor pago deve ser maior que zero.");
        }

        return new PagamentoPaciente
        {
            Id = Guid.NewGuid(),
            CriadoEm = DateTime.UtcNow,
            EmpresaId = empresaId,
            PacienteId = pacienteId,
            ContaReceberId = contaReceberId,
            FormaPagamentoId = formaPagamentoId,
            DataPagamento = dataPagamento,
            ValorPago = valorPago
        };
    }
}
