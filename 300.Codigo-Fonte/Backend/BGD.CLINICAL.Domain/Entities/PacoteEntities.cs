using BGD.CLINICAL.Domain.Common;
using BGD.CLINICAL.Domain.Enums;

namespace BGD.CLINICAL.Domain.Entities;

public sealed class Pacote : AggregateRoot
{
    private Pacote()
    {
    }

    public Pacote(Guid empresaId, string nome, string? descricao, int quantidadeAplicacoes, decimal valor)
        : base(Guid.NewGuid())
    {
        EmpresaId = empresaId;
        Nome = nome;
        Descricao = descricao;
        QuantidadeAplicacoes = quantidadeAplicacoes;
        Valor = valor;
        Ativo = true;
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
}

public sealed class ItemPacote : AggregateRoot
{
    private ItemPacote()
    {
    }

    public ItemPacote(Guid pacoteId, Guid produtoId, decimal quantidadeTotal, string unidadeMedida)
        : base(Guid.NewGuid())
    {
        PacoteId = pacoteId;
        ProdutoId = produtoId;
        QuantidadeTotal = quantidadeTotal;
        UnidadeMedida = unidadeMedida;
    }

    public Guid PacoteId { get; private set; }
    public Guid ProdutoId { get; private set; }
    public decimal QuantidadeTotal { get; private set; }
    public string UnidadeMedida { get; private set; } = string.Empty;

    public Pacote Pacote { get; private set; } = null!;
    public Produto Produto { get; private set; } = null!;
}

public sealed class CompraPaciente : AggregateRoot
{
    private CompraPaciente()
    {
    }

    public CompraPaciente(Guid empresaId, Guid pacienteId, Guid pacoteId, Guid unidadeId, DateTime dataCompra, int quantidadeAplicacoes, StatusCompraPaciente status)
        : base(Guid.NewGuid())
    {
        EmpresaId = empresaId;
        PacienteId = pacienteId;
        PacoteId = pacoteId;
        UnidadeId = unidadeId;
        DataCompra = dataCompra;
        QuantidadeAplicacoes = quantidadeAplicacoes;
        Status = status;
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
}
