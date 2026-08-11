using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Inventory.Abstractions;
using AG.CLINICAL.Domain.Constants;
using AG.CLINICAL.Domain.Entities;
using AG.CLINICAL.Domain.Exceptions;

namespace AG.CLINICAL.Application.Inventory.StockMovements;

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
        bool apenasAtivosNaoVencidos = false,
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
        bool apenasAtivosNaoVencidos = false,
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
            apenasAtivosNaoVencidos: apenasAtivosNaoVencidos,
            cancellationToken: cancellationToken);

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
                await BuildMensagemSaldoLoteInsuficienteAsync(
                    empresaId,
                    unidadeId,
                    produto.Id,
                    quantidadeEstoque,
                    lotes.Sum(lote => lote.Saldo),
                    apenasAtivosNaoVencidos,
                    cancellationToken));
        }

        return alocacoes;
    }

    private async Task<string> BuildMensagemSaldoLoteInsuficienteAsync(
        Guid empresaId,
        Guid unidadeId,
        Guid produtoId,
        decimal quantidadeSolicitada,
        decimal saldoTransferivel,
        bool apenasAtivosNaoVencidos,
        CancellationToken cancellationToken)
    {
        if (!apenasAtivosNaoVencidos)
        {
            return
                $"Estoque insuficiente nos lotes do medicamento. " +
                $"Disponível em lotes: {QuantidadeFormatter.Format(saldoTransferivel)}; " +
                $"quantidade solicitada: {QuantidadeFormatter.Format(quantidadeSolicitada)}.";
        }

        var lotesComSaldo = (await _stockBalancesRepository.ListLotBalancesAsync(
                empresaId,
                unidadeId,
                produtoId,
                cancellationToken))
            .Where(lote => lote.SaldoAtual > 0)
            .ToList();

        if (lotesComSaldo.Count == 0)
        {
            return
                "Não é possível transferir este medicamento: não há lotes com saldo na unidade de origem. " +
                "O saldo do produto não está vinculado a lotes. Verifique as movimentações ou registre uma entrada com lote.";
        }

        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        var vencidos = lotesComSaldo
            .Where(lote => lote.DataValidade < hoje)
            .OrderBy(lote => lote.DataValidade)
            .ToList();
        var inativos = lotesComSaldo
            .Where(lote => !lote.Ativo)
            .OrderBy(lote => lote.Codigo)
            .ToList();
        var saldoTotalEmLotes = lotesComSaldo.Sum(lote => lote.SaldoAtual);

        if (saldoTransferivel <= 0 && (vencidos.Count > 0 || inativos.Count > 0))
        {
            var detalhes = new List<string>();

            if (vencidos.Count > 0)
            {
                var exemplos = string.Join(
                    ", ",
                    vencidos.Take(3).Select(lote =>
                        $"{lote.Codigo} (válido até {lote.DataValidade:dd/MM/yyyy}, saldo {QuantidadeFormatter.Format(lote.SaldoAtual)})"));
                detalhes.Add($"vencidos: {exemplos}");
            }

            if (inativos.Count > 0)
            {
                var exemplos = string.Join(
                    ", ",
                    inativos.Take(3).Select(lote =>
                        $"{lote.Codigo} (saldo {QuantidadeFormatter.Format(lote.SaldoAtual)})"));
                detalhes.Add($"inativos: {exemplos}");
            }

            return
                $"Não é possível transferir: há saldo na origem ({QuantidadeFormatter.Format(saldoTotalEmLotes)}), " +
                $"mas ele está apenas em lotes bloqueados ({string.Join("; ", detalhes)}). " +
                "A transferência só usa lotes ativos e não vencidos. " +
                "Corrija a validade, reative o lote ou registre uma perda.";
        }

        return
            $"Não é possível transferir a quantidade informada. " +
            $"Disponível em lotes ativos e não vencidos: {QuantidadeFormatter.Format(saldoTransferivel)}; " +
            $"solicitado: {QuantidadeFormatter.Format(quantidadeSolicitada)}; " +
            $"saldo total em lotes: {QuantidadeFormatter.Format(saldoTotalEmLotes)}. " +
            "Reduza a quantidade ou libere lotes transferíveis.";
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
