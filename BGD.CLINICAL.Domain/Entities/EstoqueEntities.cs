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

    private Fornecedor(
        Guid empresaId,
        string nome,
        string cnpj,
        string? telefone,
        string? email)
        : base(Guid.NewGuid())
    {
        EmpresaId = empresaId;
        Nome = nome;
        Cnpj = cnpj;
        Telefone = telefone;
        Email = email;
        Ativo = true;
    }

    public Guid EmpresaId { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Cnpj { get; private set; } = string.Empty;
    public string? Telefone { get; private set; }
    public string? Email { get; private set; }
    public bool Ativo { get; private set; }

    public Empresa Empresa { get; private set; } = null!;
    public ICollection<PedidoFornecedor> Pedidos { get; private set; } = [];

    public static Fornecedor Create(
        Guid empresaId,
        string nome,
        string cnpj,
        string? telefone,
        string? email)
    {
        if (empresaId == Guid.Empty)
        {
            throw new DomainException("Informe a empresa do fornecedor.");
        }

        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DomainException("Informe o nome do fornecedor.");
        }

        if (string.IsNullOrWhiteSpace(cnpj))
        {
            throw new DomainException("Informe o CNPJ do fornecedor.");
        }

        return new Fornecedor(
            empresaId,
            nome.Trim(),
            cnpj.Trim(),
            string.IsNullOrWhiteSpace(telefone) ? null : telefone.Trim(),
            string.IsNullOrWhiteSpace(email) ? null : email.Trim());
    }

    public void UpdateDetails(string nome, string cnpj, string? telefone, string? email)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DomainException("Informe o nome do fornecedor.");
        }

        if (string.IsNullOrWhiteSpace(cnpj))
        {
            throw new DomainException("Informe o CNPJ do fornecedor.");
        }

        Nome = nome.Trim();
        Cnpj = cnpj.Trim();
        Telefone = string.IsNullOrWhiteSpace(telefone) ? null : telefone.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
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

public sealed class PedidoFornecedor : AggregateRoot
{
    private readonly List<ItemPedidoFornecedor> _itens = [];

    private PedidoFornecedor()
    {
    }

    private PedidoFornecedor(
        Guid empresaId,
        Guid fornecedorId,
        Guid unidadeId,
        TipoPedidoFornecedor tipoPedido,
        DateTime dataPedido,
        StatusPedidoFornecedor status,
        string? observacao,
        IReadOnlyList<ItemPedidoFornecedor> itens)
        : base(Guid.NewGuid())
    {
        EmpresaId = empresaId;
        FornecedorId = fornecedorId;
        UnidadeId = unidadeId;
        TipoPedido = tipoPedido;
        DataPedido = dataPedido;
        Status = status;
        Observacao = observacao;
        _itens.AddRange(itens);
        ValorTotal = itens.Sum(item => item.ValorTotal);
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
    public IReadOnlyCollection<ItemPedidoFornecedor> Itens => _itens;

    public static PedidoFornecedor Create(
        Guid empresaId,
        Guid fornecedorId,
        Guid unidadeId,
        TipoPedidoFornecedor tipoPedido,
        DateTime dataPedido,
        StatusPedidoFornecedor status,
        string? observacao,
        IReadOnlyList<ItemPedidoFornecedor> itens)
    {
        if (empresaId == Guid.Empty)
        {
            throw new DomainException("Informe a empresa do pedido.");
        }

        if (fornecedorId == Guid.Empty)
        {
            throw new DomainException("Informe o fornecedor do pedido.");
        }

        if (unidadeId == Guid.Empty)
        {
            throw new DomainException("Informe a unidade do pedido.");
        }

        if (itens.Count == 0)
        {
            throw new DomainException("Informe ao menos um item no pedido.");
        }

        if (!StatusPedidoFornecedorExtensions.IsEditable(status))
        {
            throw new DomainException(
                $"O status inicial do pedido deve ser {StatusPedidoFornecedorExtensions.EditableStatusApiList}.");
        }

        return new PedidoFornecedor(
            empresaId,
            fornecedorId,
            unidadeId,
            tipoPedido,
            dataPedido,
            status,
            string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim(),
            itens);
    }

    public void UpdateHeader(
        Guid fornecedorId,
        Guid unidadeId,
        TipoPedidoFornecedor tipoPedido,
        DateTime dataPedido,
        StatusPedidoFornecedor status,
        string? observacao)
    {
        EnsureEditable();

        if (fornecedorId == Guid.Empty)
        {
            throw new DomainException("Informe o fornecedor do pedido.");
        }

        if (unidadeId == Guid.Empty)
        {
            throw new DomainException("Informe a unidade do pedido.");
        }

        if (!StatusPedidoFornecedorExtensions.IsEditable(status))
        {
            throw new DomainException(
                $"O status do pedido deve ser {StatusPedidoFornecedorExtensions.EditableStatusApiList}.");
        }

        FornecedorId = fornecedorId;
        UnidadeId = unidadeId;
        TipoPedido = tipoPedido;
        DataPedido = dataPedido;
        Status = status;
        Observacao = string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim();
        AtualizadoEm = DateTime.UtcNow;
    }

    public void SetItens(IReadOnlyList<ItemPedidoFornecedor> itens)
    {
        EnsureEditable();

        if (itens.Count == 0)
        {
            throw new DomainException("Informe ao menos um item no pedido.");
        }

        _itens.Clear();
        _itens.AddRange(itens);
        ValorTotal = itens.Sum(item => item.ValorTotal);
        AtualizadoEm = DateTime.UtcNow;
    }

    public void UpdateDetails(
        Guid fornecedorId,
        Guid unidadeId,
        TipoPedidoFornecedor tipoPedido,
        DateTime dataPedido,
        StatusPedidoFornecedor status,
        string? observacao,
        IReadOnlyList<ItemPedidoFornecedor> itens)
    {
        UpdateHeader(
            fornecedorId,
            unidadeId,
            tipoPedido,
            dataPedido,
            status,
            observacao);

        SetItens(itens);
    }

    public void ApplyItensTotal(IReadOnlyList<ItemPedidoFornecedor> itens)
    {
        if (itens.Count == 0)
        {
            throw new DomainException("Informe ao menos um item no pedido.");
        }

        ValorTotal = itens.Sum(item => item.ValorTotal);
    }

    public void Cancel()
    {
        if (Status is StatusPedidoFornecedor.RecebidoPelaUnidade
            or StatusPedidoFornecedor.Cancelado
            or StatusPedidoFornecedor.Recusado)
        {
            throw new DomainException("Não é possível cancelar este pedido.");
        }

        Status = StatusPedidoFornecedor.Cancelado;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void MarkAsReceived()
    {
        if (Status is StatusPedidoFornecedor.RecebidoPelaUnidade
            or StatusPedidoFornecedor.Cancelado
            or StatusPedidoFornecedor.Recusado)
        {
            throw new DomainException("Não é possível receber este pedido.");
        }

        if (_itens.Count == 0)
        {
            throw new DomainException("O pedido não possui itens para recebimento.");
        }

        Status = StatusPedidoFornecedor.RecebidoPelaUnidade;
        AtualizadoEm = DateTime.UtcNow;
    }

    private void EnsureEditable()
    {
        if (Status is StatusPedidoFornecedor.RecebidoPelaUnidade
            or StatusPedidoFornecedor.Cancelado
            or StatusPedidoFornecedor.Recusado)
        {
            throw new DomainException("Não é possível editar um pedido recebido, cancelado ou recusado.");
        }
    }
}

public sealed class ItemPedidoFornecedor : AggregateRoot
{
    private ItemPedidoFornecedor()
    {
    }

    private ItemPedidoFornecedor(
        Guid pedidoFornecedorId,
        Guid produtoId,
        decimal quantidade,
        decimal valorUnitario,
        decimal valorTotal)
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

    public static ItemPedidoFornecedor Create(
        Guid pedidoFornecedorId,
        Guid produtoId,
        decimal quantidade,
        decimal? valorUnitario,
        decimal? valorTotal)
    {
        if (pedidoFornecedorId == Guid.Empty)
        {
            throw new DomainException("Informe o pedido do item.");
        }

        if (produtoId == Guid.Empty)
        {
            throw new DomainException("Informe o produto do item.");
        }

        var valores = ResolveValores(quantidade, valorUnitario, valorTotal);

        return new ItemPedidoFornecedor(
            pedidoFornecedorId,
            produtoId,
            quantidade,
            valores.ValorUnitario,
            valores.ValorTotal);
    }

    public static ItemPedidoFornecedor CreateForNewPedido(
        Guid produtoId,
        decimal quantidade,
        decimal? valorUnitario,
        decimal? valorTotal)
    {
        if (produtoId == Guid.Empty)
        {
            throw new DomainException("Informe o produto do item.");
        }

        var valores = ResolveValores(quantidade, valorUnitario, valorTotal);

        return new ItemPedidoFornecedor(
            Guid.Empty,
            produtoId,
            quantidade,
            valores.ValorUnitario,
            valores.ValorTotal);
    }

    private static (decimal ValorUnitario, decimal ValorTotal) ResolveValores(
        decimal quantidade,
        decimal? valorUnitario,
        decimal? valorTotal)
    {
        if (quantidade <= 0)
        {
            throw new DomainException("A quantidade do item deve ser maior que zero.");
        }

        var hasUnitario = valorUnitario.HasValue;
        var hasTotal = valorTotal.HasValue;

        if (!hasUnitario && !hasTotal)
        {
            throw new DomainException("Informe o valor unitário ou o valor total do item.");
        }

        if (hasUnitario && valorUnitario!.Value < 0)
        {
            throw new DomainException("O valor unitário do item não pode ser negativo.");
        }

        if (hasTotal && valorTotal!.Value < 0)
        {
            throw new DomainException("O valor total do item não pode ser negativo.");
        }

        if (hasUnitario && hasTotal)
        {
            var totalCalculado = Math.Round(quantidade * valorUnitario!.Value, 2, MidpointRounding.AwayFromZero);
            var totalInformado = Math.Round(valorTotal!.Value, 2, MidpointRounding.AwayFromZero);

            if (totalCalculado != totalInformado)
            {
                throw new DomainException("O valor total informado não confere com quantidade × valor unitário.");
            }

            return (valorUnitario.Value, totalInformado);
        }

        if (hasTotal)
        {
            var total = Math.Round(valorTotal!.Value, 2, MidpointRounding.AwayFromZero);
            var unitario = Math.Round(total / quantidade, 2, MidpointRounding.AwayFromZero);

            return (unitario, total);
        }

        var unitarioCalculado = valorUnitario!.Value;
        var totalCalculadoFromUnit = Math.Round(quantidade * unitarioCalculado, 2, MidpointRounding.AwayFromZero);

        return (unitarioCalculado, totalCalculadoFromUnit);
    }
}

public sealed class MovimentacaoEstoque : AggregateRoot
{
    private MovimentacaoEstoque()
    {
    }

    private MovimentacaoEstoque(
        Guid empresaId,
        Guid unidadeId,
        Guid produtoId,
        TipoMovimentacaoEstoque tipo,
        decimal quantidade,
        DateTime data,
        string origem,
        Guid? pedidoFornecedorId)
        : base(Guid.NewGuid())
    {
        EmpresaId = empresaId;
        UnidadeId = unidadeId;
        ProdutoId = produtoId;
        Tipo = tipo;
        Quantidade = quantidade;
        Data = data;
        Origem = origem;
        PedidoFornecedorId = pedidoFornecedorId;
    }

    private MovimentacaoEstoque(
        Guid empresaId,
        Guid unidadeId,
        Guid produtoId,
        TipoMovimentacaoEstoque tipo,
        decimal quantidade,
        DateTime data,
        string origem,
        Guid aplicacaoPacienteId,
        Guid funcionarioId)
        : base(Guid.NewGuid())
    {
        EmpresaId = empresaId;
        UnidadeId = unidadeId;
        ProdutoId = produtoId;
        Tipo = tipo;
        Quantidade = quantidade;
        Data = data;
        Origem = origem;
        AplicacaoPacienteId = aplicacaoPacienteId;
        FuncionarioId = funcionarioId;
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

    public static MovimentacaoEstoque CreateEntradaFromPedido(
        Guid empresaId,
        Guid unidadeId,
        Guid produtoId,
        Guid pedidoFornecedorId,
        decimal quantidade,
        DateTime data)
    {
        if (empresaId == Guid.Empty)
        {
            throw new DomainException("Informe a empresa da movimentação.");
        }

        if (unidadeId == Guid.Empty)
        {
            throw new DomainException("Informe a unidade da movimentação.");
        }

        if (produtoId == Guid.Empty)
        {
            throw new DomainException("Informe o produto da movimentação.");
        }

        if (pedidoFornecedorId == Guid.Empty)
        {
            throw new DomainException("Informe o pedido da movimentação.");
        }

        if (quantidade <= 0)
        {
            throw new DomainException("A quantidade da movimentação deve ser maior que zero.");
        }

        return new MovimentacaoEstoque(
            empresaId,
            unidadeId,
            produtoId,
            TipoMovimentacaoEstoque.Entrada,
            quantidade,
            data,
            "PEDIDO_FORNECEDOR",
            pedidoFornecedorId);
    }

    public static MovimentacaoEstoque CreateSaidaFromAplicacao(
        Guid empresaId,
        Guid unidadeId,
        Guid produtoId,
        Guid aplicacaoPacienteId,
        Guid funcionarioId,
        decimal quantidade,
        DateTime data)
    {
        if (empresaId == Guid.Empty)
        {
            throw new DomainException("Informe a empresa da movimentação.");
        }

        if (unidadeId == Guid.Empty)
        {
            throw new DomainException("Informe a unidade da movimentação.");
        }

        if (produtoId == Guid.Empty)
        {
            throw new DomainException("Informe o produto da movimentação.");
        }

        if (aplicacaoPacienteId == Guid.Empty)
        {
            throw new DomainException("Informe a aplicação da movimentação.");
        }

        if (funcionarioId == Guid.Empty)
        {
            throw new DomainException("Informe o funcionário da movimentação.");
        }

        if (quantidade <= 0)
        {
            throw new DomainException("A quantidade da movimentação deve ser maior que zero.");
        }

        return new MovimentacaoEstoque(
            empresaId,
            unidadeId,
            produtoId,
            TipoMovimentacaoEstoque.Saida,
            quantidade,
            data,
            "APLICACAO_PACIENTE",
            aplicacaoPacienteId,
            funcionarioId);
    }

    public static MovimentacaoEstoque CreateEntradaFromCancelamentoAplicacao(
        Guid empresaId,
        Guid unidadeId,
        Guid produtoId,
        Guid aplicacaoPacienteId,
        Guid funcionarioId,
        decimal quantidade,
        DateTime data)
    {
        if (empresaId == Guid.Empty)
        {
            throw new DomainException("Informe a empresa da movimentação.");
        }

        if (unidadeId == Guid.Empty)
        {
            throw new DomainException("Informe a unidade da movimentação.");
        }

        if (produtoId == Guid.Empty)
        {
            throw new DomainException("Informe o produto da movimentação.");
        }

        if (aplicacaoPacienteId == Guid.Empty)
        {
            throw new DomainException("Informe a aplicação da movimentação.");
        }

        if (funcionarioId == Guid.Empty)
        {
            throw new DomainException("Informe o funcionário da movimentação.");
        }

        if (quantidade <= 0)
        {
            throw new DomainException("A quantidade da movimentação deve ser maior que zero.");
        }

        return new MovimentacaoEstoque(
            empresaId,
            unidadeId,
            produtoId,
            TipoMovimentacaoEstoque.Entrada,
            quantidade,
            data,
            "APLICACAO_PACIENTE_CANCELAMENTO",
            aplicacaoPacienteId,
            funcionarioId);
    }
}
