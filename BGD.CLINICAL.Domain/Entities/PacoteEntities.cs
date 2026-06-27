using BGD.CLINICAL.Domain.Common;
using BGD.CLINICAL.Domain.Enums;
using BGD.CLINICAL.Domain.Exceptions;

namespace BGD.CLINICAL.Domain.Entities;

public sealed class Pacote : AggregateRoot
{
    private Pacote()
    {
    }

    public Guid EmpresaId { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string? Descricao { get; private set; }
    public int QuantidadeAplicacoes { get; private set; }
    public decimal Valor { get; private set; }
    public bool Ativo { get; private set; }

    public Empresa Empresa { get; private set; } = null!;
    public ICollection<ItemPacote> Itens { get; private set; } = [];
    public ICollection<CompraPaciente> Compras { get; private set; } = [];

    public static Pacote Create(
        Guid empresaId,
        string nome,
        string? descricao,
        int quantidadeAplicacoes,
        decimal valor)
    {
        if (empresaId == Guid.Empty)
        {
            throw new DomainException("Informe a empresa do pacote.");
        }

        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DomainException("Informe o nome do pacote.");
        }

        if (quantidadeAplicacoes <= 0)
        {
            throw new DomainException("A quantidade de aplicações deve ser maior que zero.");
        }

        if (valor < 0)
        {
            throw new DomainException("O valor do pacote não pode ser negativo.");
        }

        return new Pacote
        {
            Id = Guid.NewGuid(),
            CriadoEm = DateTime.UtcNow,
            EmpresaId = empresaId,
            Nome = nome.Trim(),
            Descricao = string.IsNullOrWhiteSpace(descricao) ? null : descricao.Trim(),
            QuantidadeAplicacoes = quantidadeAplicacoes,
            Valor = valor,
            Ativo = true
        };
    }

    public void UpdateDetails(string nome, string? descricao, int quantidadeAplicacoes, decimal valor)
    {
        if (!Ativo)
        {
            throw new DomainException("Pacote inativo não pode ser alterado.");
        }

        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DomainException("Informe o nome do pacote.");
        }

        if (quantidadeAplicacoes <= 0)
        {
            throw new DomainException("A quantidade de aplicações deve ser maior que zero.");
        }

        if (valor < 0)
        {
            throw new DomainException("O valor do pacote não pode ser negativo.");
        }

        Nome = nome.Trim();
        Descricao = string.IsNullOrWhiteSpace(descricao) ? null : descricao.Trim();
        QuantidadeAplicacoes = quantidadeAplicacoes;
        Valor = valor;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!Ativo)
        {
            throw new DomainException("Pacote já está inativo.");
        }

        Ativo = false;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void Reactivate()
    {
        if (Ativo)
        {
            throw new DomainException("Pacote já está ativo.");
        }

        Ativo = true;
        AtualizadoEm = DateTime.UtcNow;
    }
}

public sealed class ItemPacote : AggregateRoot
{
    private ItemPacote()
    {
    }

    public Guid PacoteId { get; private set; }
    public Guid ProdutoId { get; private set; }
    public decimal QuantidadeTotal { get; private set; }
    public string UnidadeMedida { get; private set; } = string.Empty;

    public Pacote Pacote { get; private set; } = null!;
    public Produto Produto { get; private set; } = null!;

    public static ItemPacote Create(
        Guid pacoteId,
        Guid produtoId,
        decimal quantidadeTotal,
        string unidadeMedida)
    {
        if (pacoteId == Guid.Empty || produtoId == Guid.Empty)
        {
            throw new DomainException("Informe o pacote e o produto do item.");
        }

        if (quantidadeTotal <= 0)
        {
            throw new DomainException("A quantidade do item deve ser maior que zero.");
        }

        if (string.IsNullOrWhiteSpace(unidadeMedida))
        {
            throw new DomainException("Informe a unidade de medida do item.");
        }

        return new ItemPacote
        {
            Id = Guid.NewGuid(),
            CriadoEm = DateTime.UtcNow,
            PacoteId = pacoteId,
            ProdutoId = produtoId,
            QuantidadeTotal = quantidadeTotal,
            UnidadeMedida = unidadeMedida.Trim()
        };
    }
}

public sealed class CompraPaciente : AggregateRoot
{
    private CompraPaciente()
    {
    }

    public Guid EmpresaId { get; private set; }
    public Guid PacienteId { get; private set; }
    public Guid PacoteId { get; private set; }
    public Guid UnidadeId { get; private set; }
    public DateTime DataCompra { get; private set; }
    public int QuantidadeAplicacoes { get; private set; }
    public StatusCompraPaciente Status { get; private set; }
    public string? Observacao { get; private set; }

    public Empresa Empresa { get; private set; } = null!;
    public Paciente Paciente { get; private set; } = null!;
    public Pacote Pacote { get; private set; } = null!;
    public Unidade Unidade { get; private set; } = null!;
    public ICollection<AplicacaoPaciente> Aplicacoes { get; private set; } = [];
    public ICollection<Agendamento> Agendamentos { get; private set; } = [];

    public static CompraPaciente Create(
        Guid empresaId,
        Guid pacienteId,
        Guid pacoteId,
        Guid unidadeId,
        DateTime dataCompra,
        int quantidadeAplicacoes)
    {
        if (empresaId == Guid.Empty || pacienteId == Guid.Empty || pacoteId == Guid.Empty || unidadeId == Guid.Empty)
        {
            throw new DomainException("Informe empresa, paciente, pacote e unidade da compra.");
        }

        if (quantidadeAplicacoes <= 0)
        {
            throw new DomainException("A quantidade de aplicações deve ser maior que zero.");
        }

        return new CompraPaciente
        {
            Id = Guid.NewGuid(),
            CriadoEm = DateTime.UtcNow,
            EmpresaId = empresaId,
            PacienteId = pacienteId,
            PacoteId = pacoteId,
            UnidadeId = unidadeId,
            DataCompra = dataCompra,
            QuantidadeAplicacoes = quantidadeAplicacoes,
            Status = StatusCompraPaciente.Ativo
        };
    }

    public void Cancel(string? observacao)
    {
        if (Status == StatusCompraPaciente.Cancelado)
        {
            throw new DomainException("Compra já está cancelada.");
        }

        if (Status == StatusCompraPaciente.Concluido)
        {
            throw new DomainException("Compra concluída não pode ser cancelada.");
        }

        Status = StatusCompraPaciente.Cancelado;
        Observacao = string.IsNullOrWhiteSpace(observacao) ? Observacao : observacao.Trim();
        AtualizadoEm = DateTime.UtcNow;
    }

    public void MarkAsCompleted()
    {
        if (Status == StatusCompraPaciente.Cancelado)
        {
            throw new DomainException("Compra cancelada não pode ser concluída.");
        }

        Status = StatusCompraPaciente.Concluido;
        AtualizadoEm = DateTime.UtcNow;
    }
}
