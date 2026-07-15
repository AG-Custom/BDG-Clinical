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
    public decimal Valor { get; private set; }
    public bool Ativo { get; private set; }

    public Empresa Empresa { get; private set; } = null!;
    public ICollection<ItemPacote> Itens { get; private set; } = [];
    public ICollection<CompraPaciente> Compras { get; private set; } = [];

    public static Pacote Create(
        Guid empresaId,
        string nome,
        string? descricao,
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
            Valor = valor,
            Ativo = true
        };
    }

    public void UpdateDetails(string nome, string? descricao, decimal valor)
    {
        if (!Ativo)
        {
            throw new DomainException("Pacote inativo não pode ser alterado.");
        }

        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DomainException("Informe o nome do pacote.");
        }

        if (valor < 0)
        {
            throw new DomainException("O valor do pacote não pode ser negativo.");
        }

        Nome = nome.Trim();
        Descricao = string.IsNullOrWhiteSpace(descricao) ? null : descricao.Trim();
        Valor = valor;
        AtualizadoEm = DateTime.UtcNow;
    }

    public ItemPacote AddItem(Guid produtoId, decimal quantidadeTotal, string unidadeMedida)
    {
        if (!Ativo)
        {
            throw new DomainException("Pacote inativo não pode ser alterado.");
        }

        if (Itens.Any(item => item.ProdutoId == produtoId))
        {
            throw new DomainException("Este produto já está vinculado ao pacote.");
        }

        var item = ItemPacote.Create(Id, produtoId, quantidadeTotal, unidadeMedida);
        Itens.Add(item);
        AtualizadoEm = DateTime.UtcNow;
        return item;
    }

    public void ClearItems()
    {
        if (!Ativo)
        {
            throw new DomainException("Pacote inativo não pode ser alterado.");
        }

        Itens.Clear();
        AtualizadoEm = DateTime.UtcNow;
    }

    public void ReplaceItems(IReadOnlyList<(Guid ProdutoId, decimal QuantidadeTotal, string UnidadeMedida)> items)
    {
        if (!Ativo)
        {
            throw new DomainException("Pacote inativo não pode ser alterado.");
        }

        if (items.Count == 0)
        {
            throw new DomainException("Informe ao menos um item para o pacote.");
        }

        var distinctCount = items.Select(item => item.ProdutoId).Distinct().Count();
        if (distinctCount != items.Count)
        {
            throw new DomainException("Não é permitido repetir o mesmo produto nos itens do pacote.");
        }

        Itens.Clear();
        foreach (var item in items)
        {
            Itens.Add(ItemPacote.Create(Id, item.ProdutoId, item.QuantidadeTotal, item.UnidadeMedida));
        }

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
        string? observacao = null)
    {
        if (empresaId == Guid.Empty || pacienteId == Guid.Empty || pacoteId == Guid.Empty || unidadeId == Guid.Empty)
        {
            throw new DomainException("Informe empresa, paciente, pacote e unidade da compra.");
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
            Status = StatusCompraPaciente.Ativo,
            Observacao = string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim()
        };
    }

    public decimal GetQuantidadeUtilizada(Guid produtoId)
    {
        return Aplicacoes
            .Where(aplicacao =>
                aplicacao.Realizado
                && !aplicacao.Cancelada
                && aplicacao.ProdutoId == produtoId
                && aplicacao.QuantidadeUtilizada.HasValue)
            .Sum(aplicacao => aplicacao.QuantidadeUtilizada!.Value);
    }

    public decimal GetQuantidadeRestante(Guid produtoId)
    {
        var item = Pacote?.Itens.FirstOrDefault(i => i.ProdutoId == produtoId);
        if (item is null)
        {
            return 0;
        }

        return Math.Max(0, item.QuantidadeTotal - GetQuantidadeUtilizada(produtoId));
    }

    public bool HasSaldoProdutoDisponivel()
    {
        var itens = Pacote?.Itens;
        if (itens is null || itens.Count == 0)
        {
            return false;
        }

        return itens.Any(item => GetQuantidadeRestante(item.ProdutoId) > 0);
    }

    public void EnsurePodeAplicar(Guid pacienteId, Guid? produtoId, decimal? quantidadeUtilizada)
    {
        if (Status != StatusCompraPaciente.Ativo)
        {
            throw new DomainException("Somente compras ativas podem ser utilizadas em aplicações.");
        }

        if (PacienteId != pacienteId)
        {
            throw new DomainException("A compra selecionada não pertence ao paciente informado.");
        }

        if (!HasSaldoProdutoDisponivel())
        {
            throw new DomainException("Esta compra não possui saldo de produto disponível.");
        }

        if (!produtoId.HasValue || !quantidadeUtilizada.HasValue)
        {
            return;
        }

        var item = Pacote?.Itens.FirstOrDefault(i => i.ProdutoId == produtoId.Value);
        if (item is null)
        {
            return;
        }

        var restante = GetQuantidadeRestante(produtoId.Value);
        if (quantidadeUtilizada.Value > restante)
        {
            throw new DomainException(
                $"Quantidade insuficiente no saldo do pacote. Disponível: {restante} {item.UnidadeMedida}.");
        }
    }

    public void CompleteIfExhausted()
    {
        if (Status != StatusCompraPaciente.Ativo)
        {
            return;
        }

        if (!HasSaldoProdutoDisponivel())
        {
            MarkAsCompleted();
        }
    }

    public void ReopenIfCompleted()
    {
        if (Status != StatusCompraPaciente.Concluido)
        {
            return;
        }

        if (HasSaldoProdutoDisponivel())
        {
            Status = StatusCompraPaciente.Ativo;
            AtualizadoEm = DateTime.UtcNow;
        }
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
