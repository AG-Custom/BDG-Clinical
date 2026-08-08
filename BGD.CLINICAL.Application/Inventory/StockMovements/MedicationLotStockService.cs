using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Domain.Constants;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Exceptions;

namespace BGD.CLINICAL.Application.Inventory.StockMovements;

public interface IMedicationLotStockService
{
    Task<(LoteProduto Lote, decimal QuantidadeEstoque)> ResolveEntryAsync(
        Guid empresaId,
        Guid unidadeId,
        Produto produto,
        decimal quantidadeEmbalagem,
        string loteCodigo,
        DateOnly dataValidade,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<(Guid LoteProdutoId, decimal Quantidade)>> AllocateFefoAsync(
        Guid empresaId,
        Guid unidadeId,
        Produto produto,
        decimal quantidadeEstoque,
        CancellationToken cancellationToken = default);

    Task<(Guid LoteProdutoId, decimal Quantidade)> AllocateFromLotAsync(
        Guid empresaId,
        Guid unidadeId,
        Produto produto,
        Guid loteProdutoId,
        decimal quantidadeEstoque,
        CancellationToken cancellationToken = default);

    bool RequiresLot(Produto produto);
}

public sealed class MedicationLotStockService : IMedicationLotStockService
{
    private readonly IProductLotsRepository _productLotsRepository;
    private readonly IStockBalancesRepository _stockBalancesRepository;

    public MedicationLotStockService(
        IProductLotsRepository productLotsRepository,
        IStockBalancesRepository stockBalancesRepository)
    {
        _productLotsRepository = productLotsRepository;
        _stockBalancesRepository = stockBalancesRepository;
    }

    public bool RequiresLot(Produto produto)
    {
        return produto.ControlaEstoque
            && produto.TipoProduto?.Codigo == ProductTypeCodes.Medicamento;
    }

    public async Task<(LoteProduto Lote, decimal QuantidadeEstoque)> ResolveEntryAsync(
        Guid empresaId,
        Guid unidadeId,
        Produto produto,
        decimal quantidadeEmbalagem,
        string loteCodigo,
        DateOnly dataValidade,
        CancellationToken cancellationToken = default)
    {
        if (!RequiresLot(produto))
        {
            throw new DomainException("Produto não exige controle de lote.");
        }

        if (!produto.TemConversaoMedicamento)
        {
            throw new DomainException("Configure a conversão de embalagem do medicamento antes de movimentar estoque.");
        }

        if (string.IsNullOrWhiteSpace(loteCodigo))
        {
            throw new DomainException("Informe o código do lote.");
        }

        var quantidadeEstoque = produto.ConverterEmbalagemParaEstoque(quantidadeEmbalagem);
        var existente = await _productLotsRepository.GetByCodigoAsync(
            empresaId,
            unidadeId,
            produto.Id,
            loteCodigo,
            cancellationToken);

        if (existente is not null)
        {
            if (existente.DataValidade != dataValidade)
            {
                throw new DomainException(
                    $"O lote {existente.Codigo} já existe com validade {existente.DataValidade:dd/MM/yyyy}.");
            }

            return (existente, quantidadeEstoque);
        }

        var lote = LoteProduto.Create(empresaId, unidadeId, produto.Id, loteCodigo, dataValidade);
        await _productLotsRepository.AddAsync(lote, cancellationToken);
        return (lote, quantidadeEstoque);
    }

    public async Task<IReadOnlyList<(Guid LoteProdutoId, decimal Quantidade)>> AllocateFefoAsync(
        Guid empresaId,
        Guid unidadeId,
        Produto produto,
        decimal quantidadeEstoque,
        CancellationToken cancellationToken = default)
    {
        if (quantidadeEstoque <= 0)
        {
            throw new DomainException("A quantidade deve ser maior que zero.");
        }

        if (!RequiresLot(produto))
        {
            return [];
        }

        var lotes = await _stockBalancesRepository.ListLotsWithBalanceFefoAsync(
            empresaId,
            unidadeId,
            produto.Id,
            cancellationToken);

        var restante = quantidadeEstoque;
        var alocacoes = new List<(Guid LoteProdutoId, decimal Quantidade)>();

        foreach (var lote in lotes)
        {
            if (restante <= 0)
            {
                break;
            }

            var consumir = Math.Min(lote.Saldo, restante);
            if (consumir <= 0)
            {
                continue;
            }

            alocacoes.Add((lote.LoteProdutoId, consumir));
            restante -= consumir;
        }

        if (restante > 0)
        {
            throw new DomainException(
                "Estoque insuficiente nos lotes disponíveis para a quantidade informada.");
        }

        return alocacoes;
    }

    public async Task<(Guid LoteProdutoId, decimal Quantidade)> AllocateFromLotAsync(
        Guid empresaId,
        Guid unidadeId,
        Produto produto,
        Guid loteProdutoId,
        decimal quantidadeEstoque,
        CancellationToken cancellationToken = default)
    {
        if (quantidadeEstoque <= 0)
        {
            throw new DomainException("A quantidade deve ser maior que zero.");
        }

        if (!RequiresLot(produto))
        {
            throw new DomainException("Produto não exige controle de lote.");
        }

        if (loteProdutoId == Guid.Empty)
        {
            throw new DomainException("Informe o lote do medicamento.");
        }

        var lote = await _productLotsRepository.GetByIdAndEmpresaIdAsync(
            loteProdutoId,
            empresaId,
            cancellationToken);

        if (lote is null || !lote.Ativo)
        {
            throw new DomainException("Lote não encontrado ou inativo.");
        }

        if (lote.UnidadeId != unidadeId || lote.ProdutoId != produto.Id)
        {
            throw new DomainException(
                "O lote informado não pertence ao medicamento ou à unidade selecionada.");
        }

        var saldoLote = await _stockBalancesRepository.GetSaldoByLoteAsync(
            empresaId,
            loteProdutoId,
            cancellationToken);

        if (saldoLote < quantidadeEstoque)
        {
            throw new DomainException(
                $"Estoque insuficiente no lote {lote.Codigo}. Saldo: {saldoLote} | Necessário: {quantidadeEstoque}");
        }

        return (loteProdutoId, quantidadeEstoque);
    }
}
