using BGD.CLINICAL.Domain.Common;
using BGD.CLINICAL.Domain.Enums;
using BGD.CLINICAL.Domain.Exceptions;

namespace BGD.CLINICAL.Domain.Entities;

public sealed class UnidadeMedida : AggregateRoot
{
    private UnidadeMedida()
    {
    }

    private UnidadeMedida(
        Guid empresaId,
        string nome,
        string sigla,
        TipoUnidadeMedida tipo)
        : base(Guid.NewGuid())
    {
        EmpresaId = empresaId;
        Nome = nome;
        Sigla = sigla;
        Tipo = tipo;
        Ativo = true;
    }

    public Guid EmpresaId { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Sigla { get; private set; } = string.Empty;
    public TipoUnidadeMedida Tipo { get; private set; }
    public bool Ativo { get; private set; }

    public Empresa Empresa { get; private set; } = null!;
    public ICollection<Produto> Produtos { get; private set; } = [];

    public static UnidadeMedida Create(
        Guid empresaId,
        string nome,
        string sigla,
        TipoUnidadeMedida tipo)
    {
        if (empresaId == Guid.Empty)
        {
            throw new DomainException("Informe a empresa da unidade de medida.");
        }

        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DomainException("Informe o nome da unidade de medida.");
        }

        if (string.IsNullOrWhiteSpace(sigla))
        {
            throw new DomainException("Informe a sigla da unidade de medida.");
        }

        return new UnidadeMedida(empresaId, nome.Trim(), sigla.Trim(), tipo);
    }

    public void UpdateDetails(string nome, string sigla, TipoUnidadeMedida tipo)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DomainException("Informe o nome da unidade de medida.");
        }

        if (string.IsNullOrWhiteSpace(sigla))
        {
            throw new DomainException("Informe a sigla da unidade de medida.");
        }

        Nome = nome.Trim();
        Sigla = sigla.Trim();
        Tipo = tipo;
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

public sealed class TipoProduto : AggregateRoot
{
    private TipoProduto()
    {
    }

    private TipoProduto(Guid empresaId, string nome)
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
    public ICollection<Produto> Produtos { get; private set; } = [];

    public static TipoProduto Create(Guid empresaId, string nome)
    {
        if (empresaId == Guid.Empty)
        {
            throw new DomainException("Informe a empresa do tipo de produto.");
        }

        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DomainException("Informe o nome do tipo de produto.");
        }

        return new TipoProduto(empresaId, nome.Trim());
    }

    public void UpdateDetails(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DomainException("Informe o nome do tipo de produto.");
        }

        Nome = nome.Trim();
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

public sealed class Produto : AggregateRoot
{
    private Produto()
    {
    }

    private Produto(
        Guid empresaId,
        Guid tipoProdutoId,
        Guid unidadeMedidaId,
        string nome,
        decimal estoqueMinimo)
        : base(Guid.NewGuid())
    {
        EmpresaId = empresaId;
        TipoProdutoId = tipoProdutoId;
        UnidadeMedidaId = unidadeMedidaId;
        Nome = nome;
        EstoqueMinimo = estoqueMinimo;
        Ativo = true;
    }

    public Guid EmpresaId { get; private set; }
    public Guid TipoProdutoId { get; private set; }
    public Guid UnidadeMedidaId { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public decimal EstoqueMinimo { get; private set; }
    public bool Ativo { get; private set; }

    public Empresa Empresa { get; private set; } = null!;
    public TipoProduto TipoProduto { get; private set; } = null!;
    public UnidadeMedida UnidadeMedida { get; private set; } = null!;

    public static Produto Create(
        Guid empresaId,
        Guid tipoProdutoId,
        Guid unidadeMedidaId,
        string nome,
        decimal estoqueMinimo)
    {
        if (empresaId == Guid.Empty)
        {
            throw new DomainException("Informe a empresa do produto.");
        }

        if (tipoProdutoId == Guid.Empty)
        {
            throw new DomainException("Informe o tipo do produto.");
        }

        if (unidadeMedidaId == Guid.Empty)
        {
            throw new DomainException("Informe a unidade de medida do produto.");
        }

        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DomainException("Informe o nome do produto.");
        }

        if (estoqueMinimo < 0)
        {
            throw new DomainException("O estoque mínimo não pode ser negativo.");
        }

        return new Produto(
            empresaId,
            tipoProdutoId,
            unidadeMedidaId,
            nome.Trim(),
            estoqueMinimo);
    }

    public void UpdateDetails(
        Guid tipoProdutoId,
        Guid unidadeMedidaId,
        string nome,
        decimal estoqueMinimo)
    {
        if (tipoProdutoId == Guid.Empty)
        {
            throw new DomainException("Informe o tipo do produto.");
        }

        if (unidadeMedidaId == Guid.Empty)
        {
            throw new DomainException("Informe a unidade de medida do produto.");
        }

        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DomainException("Informe o nome do produto.");
        }

        if (estoqueMinimo < 0)
        {
            throw new DomainException("O estoque mínimo não pode ser negativo.");
        }

        TipoProdutoId = tipoProdutoId;
        UnidadeMedidaId = unidadeMedidaId;
        Nome = nome.Trim();
        EstoqueMinimo = estoqueMinimo;
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
