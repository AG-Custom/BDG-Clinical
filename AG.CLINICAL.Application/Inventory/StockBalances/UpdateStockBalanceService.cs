using AG.CLINICAL.Application.Abstractions.Persistence;
using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Core.Abstractions;
using AG.CLINICAL.Application.Identity.Abstractions;
using AG.CLINICAL.Application.Inventory.Abstractions;
using AG.CLINICAL.Application.Inventory.Dtos;
using AG.CLINICAL.Application.Inventory.StockMovements;
using AG.CLINICAL.Domain.Entities;
using AG.CLINICAL.Domain.Enums;
using AG.CLINICAL.Domain.Exceptions;

namespace AG.CLINICAL.Application.Inventory.StockBalances;

public interface IUpdateStockBalanceService
{
    Task<Result<IReadOnlyList<StockMovementDto>>> ExecuteAsync(
        UpdateStockBalanceRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class UpdateStockBalanceService : IUpdateStockBalanceService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IUnitsRepository _unitsRepository;
    private readonly IProductsRepository _productsRepository;
    private readonly IProductLotsRepository _productLotsRepository;
    private readonly IStockBalancesRepository _stockBalancesRepository;
    private readonly IStockMovementsRepository _stockMovementsRepository;
    private readonly IUsersRepository _usersRepository;
    private readonly IMedicationLotStockService _medicationLotStockService;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateStockBalanceService(
        ICurrentTenantContext tenantContext,
        IUnitsRepository unitsRepository,
        IProductsRepository productsRepository,
        IProductLotsRepository productLotsRepository,
        IStockBalancesRepository stockBalancesRepository,
        IStockMovementsRepository stockMovementsRepository,
        IUsersRepository usersRepository,
        IMedicationLotStockService medicationLotStockService,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _unitsRepository = unitsRepository;
        _productsRepository = productsRepository;
        _productLotsRepository = productLotsRepository;
        _stockBalancesRepository = stockBalancesRepository;
        _stockMovementsRepository = stockMovementsRepository;
        _usersRepository = usersRepository;
        _medicationLotStockService = medicationLotStockService;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<StockMovementDto>>> ExecuteAsync(
        UpdateStockBalanceRequest request,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;

        try
        {
            if (request.UnidadeId == Guid.Empty || request.ProdutoId == Guid.Empty)
            {
                return Result<IReadOnlyList<StockMovementDto>>.Failure(
                    "Informe a unidade e o produto do saldo.");
            }

            if (request.SaldoDesejado < 0)
            {
                return Result<IReadOnlyList<StockMovementDto>>.Failure(
                    "O novo saldo não pode ser negativo.");
            }

            var observacao = request.Observacao?.Trim();
            if (string.IsNullOrWhiteSpace(observacao))
            {
                return Result<IReadOnlyList<StockMovementDto>>.Failure(
                    "Informe o motivo da correção de saldo.");
            }

            if (observacao.Length > 2000)
            {
                return Result<IReadOnlyList<StockMovementDto>>.Failure(
                    "O motivo da correção deve ter no máximo 2000 caracteres.");
            }

            var unidade = await _unitsRepository.GetByIdAndEmpresaIdAsync(
                request.UnidadeId,
                empresaId,
                cancellationToken);
            if (unidade is null || !unidade.Ativo)
            {
                return Result<IReadOnlyList<StockMovementDto>>.Failure(
                    "Unidade não encontrada ou inativa.");
            }

            var produto = await _productsRepository.GetByIdAndEmpresaIdAsync(
                request.ProdutoId,
                empresaId,
                cancellationToken);
            if (produto is null || !produto.Ativo)
            {
                return Result<IReadOnlyList<StockMovementDto>>.Failure(
                    "Produto não encontrado ou inativo.");
            }

            if (!produto.ControlaEstoque)
            {
                return Result<IReadOnlyList<StockMovementDto>>.Failure(
                    "Este produto não possui controle de estoque.");
            }

            var saldoAtual = await _stockBalancesRepository.GetSaldoByUnidadeAndProdutoAsync(
                empresaId,
                request.UnidadeId,
                request.ProdutoId,
                cancellationToken);
            var diferenca = request.SaldoDesejado - saldoAtual;

            if (diferenca == 0)
            {
                return Result<IReadOnlyList<StockMovementDto>>.Failure(
                    "O novo saldo é igual ao saldo atual.");
            }

            var usuario = await _usersRepository.GetByIdAsync(
                _tenantContext.UsuarioId,
                cancellationToken);
            var funcionarioId = usuario?.FuncionarioId;
            var data = DateTime.UtcNow;
            var movimentacoes = new List<MovimentacaoEstoque>();

            if (_medicationLotStockService.RequiresLot(produto))
            {
                if (diferenca > 0)
                {
                    if (!request.LoteProdutoId.HasValue || request.LoteProdutoId == Guid.Empty)
                    {
                        return Result<IReadOnlyList<StockMovementDto>>.Failure(
                            "Selecione o lote que receberá o acréscimo de saldo.");
                    }

                    var lote = await _productLotsRepository.GetByIdAndEmpresaIdAsync(
                        request.LoteProdutoId.Value,
                        empresaId,
                        cancellationToken);
                    if (lote is null || !lote.Ativo
                        || lote.UnidadeId != request.UnidadeId
                        || lote.ProdutoId != request.ProdutoId)
                    {
                        return Result<IReadOnlyList<StockMovementDto>>.Failure(
                            "O lote selecionado não pertence ao medicamento e à unidade informados.");
                    }

                    var movimentacao = CriarMovimentacao(
                        empresaId, request, diferenca, data, funcionarioId, observacao);
                    movimentacao.AssignLote(lote.Id, CalcularQuantidadeEmbalagem(produto, diferenca));
                    movimentacoes.Add(movimentacao);
                }
                else
                {
                    var alocacoes = await _medicationLotStockService.AllocateFefoAsync(
                        empresaId,
                        request.UnidadeId,
                        produto,
                        Math.Abs(diferenca),
                        apenasAtivosNaoVencidos: false,
                        cancellationToken: cancellationToken);

                    foreach (var alocacao in alocacoes)
                    {
                        var movimentacao = CriarMovimentacao(
                            empresaId, request, -alocacao.Quantidade, data, funcionarioId, observacao);
                        movimentacao.AssignLote(
                            alocacao.LoteProdutoId,
                            CalcularQuantidadeEmbalagem(produto, alocacao.Quantidade));
                        movimentacoes.Add(movimentacao);
                    }
                }
            }
            else
            {
                movimentacoes.Add(CriarMovimentacao(
                    empresaId, request, diferenca, data, funcionarioId, observacao));
            }

            await _stockMovementsRepository.AddRangeAsync(movimentacoes, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var resultado = new List<StockMovementDto>();
            foreach (var movimentacao in movimentacoes)
            {
                await _auditLogsService.RegisterEntityChangeAsync(
                    empresaId,
                    _tenantContext.UsuarioId,
                    nameof(MovimentacaoEstoque),
                    movimentacao.Id,
                    AcaoAuditoria.GerarMovimentacao,
                    dadosNovos: StockMovementsAuditSerializer.Serialize(movimentacao),
                    cancellationToken: cancellationToken);

                var persisted = await _stockMovementsRepository.GetByIdAndEmpresaIdWithDetailsAsync(
                    movimentacao.Id,
                    empresaId,
                    cancellationToken);
                resultado.Add(StockMovementsMapper.Map(persisted ?? movimentacao));
            }

            return Result<IReadOnlyList<StockMovementDto>>.Success(resultado);
        }
        catch (DomainException exception)
        {
            return Result<IReadOnlyList<StockMovementDto>>.Failure(exception.Message);
        }
    }

    private static MovimentacaoEstoque CriarMovimentacao(
        Guid empresaId,
        UpdateStockBalanceRequest request,
        decimal diferenca,
        DateTime data,
        Guid? funcionarioId,
        string observacao)
    {
        return MovimentacaoEstoque.CreateCorrecaoSaldo(
            empresaId,
            request.UnidadeId,
            request.ProdutoId,
            diferenca,
            data,
            funcionarioId,
            observacao);
    }

    private static decimal? CalcularQuantidadeEmbalagem(Produto produto, decimal quantidadeEstoque)
    {
        return produto.FatorEmbalagemParaEstoque is > 0
            ? Math.Abs(quantidadeEstoque) / produto.FatorEmbalagemParaEstoque.Value
            : null;
    }
}
