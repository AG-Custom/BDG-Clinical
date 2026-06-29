using BGD.CLINICAL.Application.Abstractions.Storage;
using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Application.Inventory.Dtos;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using BGD.CLINICAL.Domain.Exceptions;

namespace BGD.CLINICAL.Application.Inventory.SupplierOrders;

public interface IReceiveSupplierOrdersService
{
    Task<Result<SupplierOrderDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

public sealed class ReceiveSupplierOrdersService : IReceiveSupplierOrdersService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly ISupplierOrdersRepository _supplierOrdersRepository;
    private readonly IStockMovementsRepository _stockMovementsRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IObjectStorageService _objectStorageService;

    public ReceiveSupplierOrdersService(
        ICurrentTenantContext tenantContext,
        ISupplierOrdersRepository supplierOrdersRepository,
        IStockMovementsRepository stockMovementsRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork,
        IObjectStorageService objectStorageService)
    {
        _tenantContext = tenantContext;
        _supplierOrdersRepository = supplierOrdersRepository;
        _stockMovementsRepository = stockMovementsRepository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
        _objectStorageService = objectStorageService;
    }

    public async Task<Result<SupplierOrderDto>> ExecuteAsync(
        Guid id,
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

            var movimentacoes = pedido.Itens
                .Select(item => MovimentacaoEstoque.CreateEntradaFromPedido(
                    empresaId,
                    pedido.UnidadeId,
                    item.ProdutoId,
                    pedido.Id,
                    item.Quantidade,
                    dataRecebimento))
                .ToList();

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
