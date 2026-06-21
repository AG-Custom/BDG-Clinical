using BGD.CLINICAL.Domain.Common;
using BGD.CLINICAL.Domain.Enums;

namespace BGD.CLINICAL.Domain.Entities;

public sealed class Produto : AggregateRoot
{
    private Produto()
    {
    }

    public Produto(Guid empresaId, string nome, TipoProduto tipo, string unidadeMedida, decimal estoqueMinimo)
        : base(Guid.NewGuid())
    {
        EmpresaId = empresaId;
        Nome = nome;
        Tipo = tipo;
        UnidadeMedida = unidadeMedida;
        EstoqueMinimo = estoqueMinimo;
        Ativo = true;
    }

    public Guid EmpresaId { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public TipoProduto Tipo { get; private set; }
    public string UnidadeMedida { get; private set; } = string.Empty;
    public decimal EstoqueMinimo { get; private set; }
    public bool Ativo { get; private set; }

    public Empresa Empresa { get; private set; } = null!;
}

public sealed class Fornecedor : AggregateRoot
{
    private Fornecedor()
    {
    }

    public Fornecedor(Guid empresaId, string nome, string? telefone, string? email, string? cnpj)
        : base(Guid.NewGuid())
    {
        EmpresaId = empresaId;
        Nome = nome;
        Telefone = telefone;
        Email = email;
        Cnpj = cnpj;
        Ativo = true;
    }

    public Guid EmpresaId { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string? Telefone { get; private set; }
    public string? Email { get; private set; }
    public string? Cnpj { get; private set; }
    public bool Ativo { get; private set; }

    public Empresa Empresa { get; private set; } = null!;
    public ICollection<PedidoFornecedor> Pedidos { get; private set; } = [];
}

public sealed class PedidoFornecedor : AggregateRoot
{
    private PedidoFornecedor()
    {
    }

    public PedidoFornecedor(Guid empresaId, Guid fornecedorId, Guid unidadeId, TipoPedidoFornecedor tipoPedido, DateTime dataPedido, StatusPedidoFornecedor status, decimal valorTotal)
        : base(Guid.NewGuid())
    {
        EmpresaId = empresaId;
        FornecedorId = fornecedorId;
        UnidadeId = unidadeId;
        TipoPedido = tipoPedido;
        DataPedido = dataPedido;
        Status = status;
        ValorTotal = valorTotal;
    }

    public Guid EmpresaId { get; private set; }
    public Guid FornecedorId { get; private set; }
    public Guid UnidadeId { get; private set; }
    public TipoPedidoFornecedor TipoPedido { get; private set; }
    public DateTime DataPedido { get; private set; }
    public StatusPedidoFornecedor Status { get; private set; }
    public decimal ValorTotal { get; private set; }
    public string? Observacao { get; private set; }

    public Empresa Empresa { get; private set; } = null!;
    public Fornecedor Fornecedor { get; private set; } = null!;
    public Unidade Unidade { get; private set; } = null!;
    public ICollection<ItemPedidoFornecedor> Itens { get; private set; } = [];
}

public sealed class ItemPedidoFornecedor : AggregateRoot
{
    private ItemPedidoFornecedor()
    {
    }

    public ItemPedidoFornecedor(Guid pedidoFornecedorId, Guid produtoId, decimal quantidade, decimal valorUnitario, decimal valorTotal)
        : base(Guid.NewGuid())
    {
        PedidoFornecedorId = pedidoFornecedorId;
        ProdutoId = produtoId;
        Quantidade = quantidade;
        ValorUnitario = valorUnitario;
        ValorTotal = valorTotal;
    }

    public Guid PedidoFornecedorId { get; private set; }
    public Guid ProdutoId { get; private set; }
    public decimal Quantidade { get; private set; }
    public decimal ValorUnitario { get; private set; }
    public decimal ValorTotal { get; private set; }

    public PedidoFornecedor PedidoFornecedor { get; private set; } = null!;
    public Produto Produto { get; private set; } = null!;
}

public sealed class MovimentacaoEstoque : AggregateRoot
{
    private MovimentacaoEstoque()
    {
    }

    public MovimentacaoEstoque(Guid empresaId, Guid unidadeId, Guid produtoId, TipoMovimentacaoEstoque tipo, decimal quantidade, DateTime data, string origem)
        : base(Guid.NewGuid())
    {
        EmpresaId = empresaId;
        UnidadeId = unidadeId;
        ProdutoId = produtoId;
        Tipo = tipo;
        Quantidade = quantidade;
        Data = data;
        Origem = origem;
    }

    public Guid EmpresaId { get; private set; }
    public Guid UnidadeId { get; private set; }
    public Guid ProdutoId { get; private set; }
    public TipoMovimentacaoEstoque Tipo { get; private set; }
    public decimal Quantidade { get; private set; }
    public DateTime Data { get; private set; }
    public string Origem { get; private set; } = string.Empty;
    public Guid? FuncionarioId { get; private set; }
    public Guid? AplicacaoPacienteId { get; private set; }
    public Guid? PedidoFornecedorId { get; private set; }
    public string? Observacao { get; private set; }

    public Empresa Empresa { get; private set; } = null!;
    public Unidade Unidade { get; private set; } = null!;
    public Produto Produto { get; private set; } = null!;
    public Funcionario? Funcionario { get; private set; }
    public AplicacaoPaciente? AplicacaoPaciente { get; private set; }
    public PedidoFornecedor? PedidoFornecedor { get; private set; }
}
