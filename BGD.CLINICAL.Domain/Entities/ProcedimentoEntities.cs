using BGD.CLINICAL.Domain.Common;
using BGD.CLINICAL.Domain.Exceptions;

namespace BGD.CLINICAL.Domain.Entities;

public sealed class Procedimento : AggregateRoot
{
    private Procedimento()
    {
    }

    private Procedimento(
        Guid empresaId,
        string nome,
        Guid? produtoAplicadoId,
        string? observacoes)
        : base(Guid.NewGuid())
    {
        EmpresaId = empresaId;
        Nome = nome;
        ProdutoAplicadoId = produtoAplicadoId;
        Observacoes = observacoes;
        Ativo = true;
    }

    public Guid EmpresaId { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public Guid? ProdutoAplicadoId { get; private set; }
    public string? Observacoes { get; private set; }
    public bool Ativo { get; private set; }

    public Empresa Empresa { get; private set; } = null!;
    public Produto? ProdutoAplicado { get; private set; }
    public ICollection<ItemProcedimento> Itens { get; private set; } = [];

    public static Procedimento Create(
        Guid empresaId,
        string nome,
        Guid? produtoAplicadoId,
        string? observacoes,
        IReadOnlyList<(Guid ProdutoId, decimal Quantidade)> itens)
    {
        ValidateCommon(empresaId, nome, produtoAplicadoId, observacoes, itens);

        var procedimento = new Procedimento(
            empresaId,
            nome.Trim(),
            produtoAplicadoId,
            string.IsNullOrWhiteSpace(observacoes) ? null : observacoes.Trim());

        foreach (var item in itens)
        {
            procedimento.Itens.Add(new ItemProcedimento(procedimento.Id, item.ProdutoId, item.Quantidade));
        }

        return procedimento;
    }

    public void UpdateDetails(
        string nome,
        Guid? produtoAplicadoId,
        string? observacoes,
        IReadOnlyList<(Guid ProdutoId, decimal Quantidade)> itens)
    {
        ValidateCommon(EmpresaId, nome, produtoAplicadoId, observacoes, itens);

        Nome = nome.Trim();
        ProdutoAplicadoId = produtoAplicadoId;
        Observacoes = string.IsNullOrWhiteSpace(observacoes) ? null : observacoes.Trim();

        Itens.Clear();
        foreach (var item in itens)
        {
            Itens.Add(new ItemProcedimento(Id, item.ProdutoId, item.Quantidade));
        }

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

    private static void ValidateCommon(
        Guid empresaId,
        string nome,
        Guid? produtoAplicadoId,
        string? observacoes,
        IReadOnlyList<(Guid ProdutoId, decimal Quantidade)> itens)
    {
        if (empresaId == Guid.Empty)
        {
            throw new DomainException("Informe a empresa do procedimento.");
        }

        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DomainException("Informe o nome do procedimento.");
        }

        if (nome.Length > 200)
        {
            throw new DomainException("O nome do procedimento deve ter no máximo 200 caracteres.");
        }

        if (produtoAplicadoId.HasValue && produtoAplicadoId.Value == Guid.Empty)
        {
            throw new DomainException("Produto aplicado inválido.");
        }

        if (!string.IsNullOrWhiteSpace(observacoes) && observacoes.Length > 2000)
        {
            throw new DomainException("As observações devem ter no máximo 2000 caracteres.");
        }

        if (!produtoAplicadoId.HasValue && itens.Count == 0)
        {
            throw new DomainException("Informe o produto aplicado ou ao menos um item consumido.");
        }

        if (itens.Count > 0)
        {
            var duplicated = itens.GroupBy(item => item.ProdutoId).Any(group => group.Count() > 1);
            if (duplicated)
            {
                throw new DomainException("Não é permitido repetir o mesmo produto nos itens do procedimento.");
            }

            foreach (var item in itens)
            {
                if (item.ProdutoId == Guid.Empty)
                {
                    throw new DomainException("Informe o produto de cada item do procedimento.");
                }

                if (item.Quantidade <= 0)
                {
                    throw new DomainException("A quantidade de cada item deve ser maior que zero.");
                }

                if (produtoAplicadoId.HasValue && item.ProdutoId == produtoAplicadoId.Value)
                {
                    throw new DomainException("O produto aplicado não pode constar na lista de itens consumidos.");
                }
            }
        }
    }
}

public sealed class ItemProcedimento : AggregateRoot
{
    private ItemProcedimento()
    {
    }

    public ItemProcedimento(Guid procedimentoId, Guid produtoId, decimal quantidade)
        : base(Guid.NewGuid())
    {
        ProcedimentoId = procedimentoId;
        ProdutoId = produtoId;
        Quantidade = quantidade;
    }

    public Guid ProcedimentoId { get; private set; }
    public Guid ProdutoId { get; private set; }
    public decimal Quantidade { get; private set; }

    public Procedimento Procedimento { get; private set; } = null!;
    public Produto Produto { get; private set; } = null!;
}
