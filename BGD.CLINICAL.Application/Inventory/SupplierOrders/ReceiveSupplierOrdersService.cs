using BGD.CLINICAL.Application.Abstractions.Storage;
using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Application.Inventory.Dtos;
using BGD.CLINICAL.Application.Inventory.StockMovements;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using BGD.CLINICAL.Domain.Exceptions;

namespace BGD.CLINICAL.Application.Inventory.SupplierOrders;

public interface IReceiveSupplierOrdersService
{
    Task<Result<SupplierOrderDto>> ExecuteAsync(
        Guid id,
        ReceiveSupplierOrderRequest? request = null,
        CancellationToken cancellationToken = default);
}

public sealed class ReceiveSupplierOrdersService : IReceiveSupplierOrdersService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly ISupplierOrdersRepository _supplierOrdersRepository;
    private readonly IProductsRepository _productsRepository;
    private readonly IStockMovementsRepository _stockMovementsRepository;
    private readonly IMedicationLotStockService _medicationLotStockService;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IObjectStorageService _objectStorageService;

    public ReceiveSupplierOrdersService(
        ICurrentTenantContext tenantContext,
        ISupplierOrdersRepository supplierOrdersRepository,
        IProductsRepository productsRepository,
        IStockMovementsRepository stockMovementsRepository,
        IMedicationLotStockService medicationLotStockService,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork,
        IObjectStorageService objectStorageService)
    {
        _tenantContext = tenantContext;
        _supplierOrdersRepository = supplierOrdersRepository;
        _productsRepository = productsRepository;
        _stockMovementsRepository = stockMovementsRepository;
        _medicationLotStockService = medicationLotStockService;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
        _objectStorageService = objectStorageService;
    }

    public async Task<Result<SupplierOrderDto>> ExecuteAsync(
        Guid id,
        ReceiveSupplierOrderRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;
        var pedido = await _supplierOrdersRepository.GetByIdAndEmpresaIdWithItensAsync(id, empresaId, cancellationToken);

        if (pedido is null)
        {
            return Result<SupplierOrderDto>.Failure("Pedido não encontrado.");
        }

        try
        {
            var dadosAnteriores = SupplierOrdersAuditSerializer.Serialize(pedido);
            var dataRecebimento = DateTime.UtcNow;
            var lotesPorProduto = (request?.Itens ?? [])
                .GroupBy(item => item.ProdutoId)
                .ToDictionary(group => group.Key, group => group.First());

            var produtoIds = pedido.Itens.Select(item => item.ProdutoId).Distinct().ToList();
            var produtos = await _productsRepository.GetActiveByIdsAndEmpresaIdAsync(
                empresaId,
                produtoIds,
                cancellationToken);
            var produtosPorId = produtos.ToDictionary(produto => produto.Id);

            var movimentacoes = new List<MovimentacaoEstoque>();

            foreach (var item in pedido.Itens)
            {
                if (!produtosPorId.TryGetValue(item.ProdutoId, out var produto))
                {
                    return Result<SupplierOrderDto>.Failure($"Produto do item do pedido não encontrado: {item.ProdutoId}.");
                }

                if (_medicationLotStockService.RequiresLot(produto))
                {
                    if (!lotesPorProduto.TryGetValue(item.ProdutoId, out var loteInfo)
                        || string.IsNullOrWhiteSpace(loteInfo.LoteCodigo)
                        || loteInfo.DataValidade is null)
                    {
                        return Result<SupplierOrderDto>.Failure(
                            $"Informe lote e validade para o medicamento {produto.Nome}.");
                    }

                    var quantidadeEmbalagem = loteInfo.QuantidadeEmbalagem ?? item.Quantidade;
                    var entry = await _medicationLotStockService.ResolveEntryAsync(
                        empresaId,
                        pedido.UnidadeId,
                        produto,
                        quantidadeEmbalagem,
                        loteInfo.LoteCodigo,
                        loteInfo.DataValidade.Value,
                        cancellationToken);

                    var movimentacao = MovimentacaoEstoque.CreateEntradaFromPedido(
                        empresaId,
                        pedido.UnidadeId,
                        item.ProdutoId,
                        pedido.Id,
                        entry.QuantidadeEstoque,
                        dataRecebimento);
                    movimentacao.AssignLote(entry.Lote.Id, quantidadeEmbalagem);
                    movimentacoes.Add(movimentacao);
                }
                else
                {
                    movimentacoes.Add(MovimentacaoEstoque.CreateEntradaFromPedido(
                        empresaId,
                        pedido.UnidadeId,
                        item.ProdutoId,
                        pedido.Id,
                        item.Quantidade,
                        dataRecebimento));
                }
            }

            pedido.MarkAsReceived();
            _supplierOrdersRepository.Update(pedido);
            await _stockMovementsRepository.AddRangeAsync(movimentacoes, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditLogsService.RegisterEntityChangeAsync(
                empresaId,
                _tenantContext.UsuarioId,
                nameof(PedidoFornecedor),
                pedido.Id,
                AcaoAuditoria.Editar,
                dadosAnteriores: dadosAnteriores,
                dadosNovos: SupplierOrdersAuditSerializer.Serialize(pedido),
                cancellationToken: cancellationToken);

            foreach (var movimentacao in movimentacoes)
            {
                await _auditLogsService.RegisterEntityChangeAsync(
                    empresaId,
                    _tenantContext.UsuarioId,
                    nameof(MovimentacaoEstoque),
                    movimentacao.Id,
                    AcaoAuditoria.GerarMovimentacao,
                    dadosNovos: SupplierOrdersAuditSerializer.Serialize(movimentacao),
                    cancellationToken: cancellationToken);
            }

            var persisted = await _supplierOrdersRepository.GetByIdAndEmpresaIdWithItensAsync(
                pedido.Id,
                empresaId,
                cancellationToken);

            return Result<SupplierOrderDto>.Success(
                SupplierOrdersMapper.Map(persisted ?? pedido, _objectStorageService));
        }
        catch (DomainException exception)
        {
            return Result<SupplierOrderDto>.Failure(exception.Message);
        }
    }
}
