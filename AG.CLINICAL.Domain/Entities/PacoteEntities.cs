using System.Globalization;
using AG.CLINICAL.Domain.Common;
using AG.CLINICAL.Domain.Enums;
using AG.CLINICAL.Domain.Exceptions;

namespace AG.CLINICAL.Domain.Entities;

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

    /// <summary>
    /// Pacotes auxiliares criados na migração de saldo (ex.: "Migração — Paciente — Produto").
    /// Não fazem parte do catálogo comercial e não devem aparecer em GET /api/packages.
    /// </summary>
    public static bool NomeIndicaMigracao(string? nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            return false;
        }

        var trimmed = nome.Trim();
        return trimmed.StartsWith("Migração", StringComparison.OrdinalIgnoreCase)
            || trimmed.StartsWith("Migracao", StringComparison.OrdinalIgnoreCase);
    }

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

    public void UpdateItemQuantidade(
        Guid produtoId,
        decimal quantidadeTotal,
        decimal quantidadeUtilizadaDesejada,
        decimal quantidadeUtilizadaNasAplicacoes)
    {
        if (!Ativo)
        {
            throw new DomainException("Pacote inativo não pode ser alterado.");
        }

        var item = Itens.FirstOrDefault(i => i.ProdutoId == produtoId);
        if (item is null)
        {
            throw new DomainException("Produto não encontrado neste pacote.");
        }

        item.UpdateSaldo(quantidadeTotal, quantidadeUtilizadaDesejada, quantidadeUtilizadaNasAplicacoes);
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

    /// <summary>
    /// Consumo já utilizado fora das aplicações do sistema (ex.: correção de migração).
    /// O utilizado total = base + soma das aplicações realizadas.
    /// </summary>
    public decimal QuantidadeUtilizadaBase { get; private set; }

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
            QuantidadeUtilizadaBase = 0,
            UnidadeMedida = unidadeMedida.Trim()
        };
    }

    public void UpdateSaldo(
        decimal quantidadeTotal,
        decimal quantidadeUtilizadaDesejada,
        decimal quantidadeUtilizadaNasAplicacoes)
    {
        if (quantidadeTotal <= 0)
        {
            throw new DomainException("A quantidade do item deve ser maior que zero.");
        }

        if (quantidadeUtilizadaDesejada < 0)
        {
            throw new DomainException("A quantidade utilizada não pode ser negativa.");
        }

        if (quantidadeUtilizadaNasAplicacoes < 0)
        {
            throw new DomainException("A quantidade utilizada nas aplicações é inválida.");
        }

        if (quantidadeTotal < quantidadeUtilizadaDesejada)
        {
            throw new DomainException(
                $"A quantidade contratada não pode ser menor que a já utilizada ({FormatarQuantidadeSaldo(quantidadeUtilizadaDesejada)} {UnidadeMedida}).");
        }

        QuantidadeTotal = quantidadeTotal;
        QuantidadeUtilizadaBase = quantidadeUtilizadaDesejada - quantidadeUtilizadaNasAplicacoes;
        AtualizadoEm = DateTime.UtcNow;
    }

    private static string FormatarQuantidadeSaldo(decimal quantidade)
    {
        return quantidade.ToString("0.####", CultureInfo.GetCultureInfo("pt-BR"));
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
    public ICollection<ItemCompraPaciente> Itens { get; private set; } = [];
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

    public void CopiarItensDoPacote(IEnumerable<ItemPacote> itensPacote)
    {
        if (Itens.Count > 0)
        {
            return;
        }

        foreach (var item in itensPacote)
        {
            Itens.Add(ItemCompraPaciente.Create(
                Id,
                item.ProdutoId,
                item.QuantidadeTotal,
                0,
                item.UnidadeMedida));
        }

        if (Itens.Count == 0)
        {
            throw new DomainException("O pacote da compra não possui itens para copiar.");
        }
    }

    public ItemCompraPaciente AdicionarItemMigrado(
        Guid produtoId,
        decimal quantidadeContratada,
        decimal quantidadeUtilizadaBase,
        string unidadeMedida)
    {
        if (Itens.Any(item => item.ProdutoId == produtoId))
        {
            throw new DomainException("Este produto já está vinculado à compra.");
        }

        var item = ItemCompraPaciente.Create(
            Id,
            produtoId,
            quantidadeContratada,
            quantidadeUtilizadaBase,
            unidadeMedida);
        Itens.Add(item);
        AtualizadoEm = DateTime.UtcNow;
        return item;
    }

    public void UpdateItemSaldo(
        Guid produtoId,
        decimal quantidadeContratada,
        decimal quantidadeUtilizadaDesejada,
        decimal quantidadeUtilizadaNasAplicacoes)
    {
        if (Status == StatusCompraPaciente.Cancelado)
        {
            throw new DomainException("Compra cancelada não pode ter o saldo alterado.");
        }

        if (Itens.Count == 0 && Pacote?.Itens is { Count: > 0 } itensPacote)
        {
            CopiarItensDoPacote(itensPacote);
        }

        var item = Itens.FirstOrDefault(i => i.ProdutoId == produtoId);
        if (item is null)
        {
            throw new DomainException("Produto não encontrado nesta compra.");
        }

        item.UpdateSaldo(quantidadeContratada, quantidadeUtilizadaDesejada, quantidadeUtilizadaNasAplicacoes);
        AtualizadoEm = DateTime.UtcNow;
    }

    public decimal GetQuantidadeUtilizadaNasAplicacoes(Guid produtoId)
    {
        return Aplicacoes
            .Where(aplicacao =>
                aplicacao.Realizado
                && !aplicacao.Cancelada
                && aplicacao.ProdutoId == produtoId
                && aplicacao.QuantidadeUtilizada.HasValue)
            .Sum(aplicacao => aplicacao.QuantidadeUtilizada!.Value);
    }

    public decimal GetQuantidadeUtilizada(Guid produtoId)
    {
        var baseUtilizada = ObterBaseUtilizada(produtoId);
        return baseUtilizada + GetQuantidadeUtilizadaNasAplicacoes(produtoId);
    }

    public decimal GetQuantidadeRestante(Guid produtoId)
    {
        var contratada = ObterQuantidadeContratada(produtoId);
        if (contratada is null)
        {
            return 0;
        }

        return Math.Max(0, contratada.Value - GetQuantidadeUtilizada(produtoId));
    }

    public bool HasSaldoProdutoDisponivel()
    {
        var produtoIds = ListarProdutoIdsContrato();
        if (produtoIds.Count == 0)
        {
            return false;
        }

        return produtoIds.Any(produtoId => GetQuantidadeRestante(produtoId) > 0);
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

        if (quantidadeUtilizada.Value <= 0)
        {
            throw new DomainException("A quantidade solicitada deve ser maior que zero.");
        }

        var unidade = ObterUnidadeMedida(produtoId.Value);
        if (unidade is null)
        {
            throw new DomainException("O produto aplicado não existe nos itens desta compra.");
        }

        var restante = GetQuantidadeRestante(produtoId.Value);
        if (quantidadeUtilizada.Value > restante)
        {
            throw new DomainException(
                $"Quantidade insuficiente no saldo do pacote. Disponível: {FormatarQuantidadeSaldo(restante)} {unidade}.");
        }
    }

    public string? ObterUnidadeMedida(Guid produtoId)
    {
        var itemCompra = Itens.FirstOrDefault(item => item.ProdutoId == produtoId);
        if (itemCompra is not null)
        {
            return itemCompra.UnidadeMedida;
        }

        return Pacote?.Itens.FirstOrDefault(item => item.ProdutoId == produtoId)?.UnidadeMedida;
    }

    private decimal ObterBaseUtilizada(Guid produtoId)
    {
        var itemCompra = Itens.FirstOrDefault(item => item.ProdutoId == produtoId);
        if (itemCompra is not null)
        {
            return itemCompra.QuantidadeUtilizadaBase;
        }

        return Pacote?.Itens.FirstOrDefault(item => item.ProdutoId == produtoId)?.QuantidadeUtilizadaBase ?? 0m;
    }

    private decimal? ObterQuantidadeContratada(Guid produtoId)
    {
        var itemCompra = Itens.FirstOrDefault(item => item.ProdutoId == produtoId);
        if (itemCompra is not null)
        {
            return itemCompra.QuantidadeContratada;
        }

        var itemPacote = Pacote?.Itens.FirstOrDefault(item => item.ProdutoId == produtoId);
        return itemPacote?.QuantidadeTotal;
    }

    private List<Guid> ListarProdutoIdsContrato()
    {
        if (Itens.Count > 0)
        {
            return Itens.Select(item => item.ProdutoId).ToList();
        }

        return (Pacote?.Itens ?? []).Select(item => item.ProdutoId).ToList();
    }

    private static string FormatarQuantidadeSaldo(decimal quantidade)
    {
        return quantidade.ToString("0.####", CultureInfo.GetCultureInfo("pt-BR"));
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

public sealed class ItemCompraPaciente : AggregateRoot
{
    private ItemCompraPaciente()
    {
    }

    public Guid CompraPacienteId { get; private set; }
    public Guid ProdutoId { get; private set; }
    public decimal QuantidadeContratada { get; private set; }
    public decimal QuantidadeUtilizadaBase { get; private set; }
    public string UnidadeMedida { get; private set; } = string.Empty;

    public CompraPaciente CompraPaciente { get; private set; } = null!;
    public Produto Produto { get; private set; } = null!;

    public static ItemCompraPaciente Create(
        Guid compraPacienteId,
        Guid produtoId,
        decimal quantidadeContratada,
        decimal quantidadeUtilizadaBase,
        string unidadeMedida)
    {
        if (compraPacienteId == Guid.Empty || produtoId == Guid.Empty)
        {
            throw new DomainException("Informe a compra e o produto do item.");
        }

        if (quantidadeContratada <= 0)
        {
            throw new DomainException("A quantidade contratada deve ser maior que zero.");
        }

        if (quantidadeUtilizadaBase < 0)
        {
            throw new DomainException("A quantidade utilizada base não pode ser negativa.");
        }

        if (string.IsNullOrWhiteSpace(unidadeMedida))
        {
            throw new DomainException("Informe a unidade de medida do item.");
        }

        return new ItemCompraPaciente
        {
            Id = Guid.NewGuid(),
            CriadoEm = DateTime.UtcNow,
            CompraPacienteId = compraPacienteId,
            ProdutoId = produtoId,
            QuantidadeContratada = quantidadeContratada,
            QuantidadeUtilizadaBase = quantidadeUtilizadaBase,
            UnidadeMedida = unidadeMedida.Trim()
        };
    }

    public void UpdateSaldo(
        decimal quantidadeContratada,
        decimal quantidadeUtilizadaDesejada,
        decimal quantidadeUtilizadaNasAplicacoes)
    {
        if (quantidadeContratada <= 0)
        {
            throw new DomainException("A quantidade contratada deve ser maior que zero.");
        }

        if (quantidadeUtilizadaDesejada < 0)
        {
            throw new DomainException("A quantidade utilizada não pode ser negativa.");
        }

        if (quantidadeUtilizadaNasAplicacoes < 0)
        {
            throw new DomainException("A quantidade utilizada nas aplicações é inválida.");
        }

        if (quantidadeContratada < quantidadeUtilizadaDesejada)
        {
            throw new DomainException(
                $"A quantidade contratada não pode ser menor que a já utilizada ({FormatarQuantidadeSaldo(quantidadeUtilizadaDesejada)} {UnidadeMedida}).");
        }

        var baseCalculada = quantidadeUtilizadaDesejada - quantidadeUtilizadaNasAplicacoes;
        if (baseCalculada < 0)
        {
            throw new DomainException("A quantidade utilizada não pode ser menor que as aplicações já registradas.");
        }

        QuantidadeContratada = quantidadeContratada;
        QuantidadeUtilizadaBase = baseCalculada;
        AtualizadoEm = DateTime.UtcNow;
    }

    private static string FormatarQuantidadeSaldo(decimal quantidade)
    {
        return quantidade.ToString("0.####", CultureInfo.GetCultureInfo("pt-BR"));
    }
}
