using BGD.CLINICAL.Application.Abstractions.Storage;
using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Core.Abstractions;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Application.Inventory.Dtos;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using BGD.CLINICAL.Domain.Exceptions;

namespace BGD.CLINICAL.Application.Inventory.SupplierOrders;

public interface IUpdateSupplierOrdersService
{
    Task<Result<SupplierOrderDto>> ExecuteAsync(
        Guid id,
        UpdateSupplierOrderRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class UpdateSupplierOrdersService : IUpdateSupplierOrdersService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly ISupplierOrdersRepository _supplierOrdersRepository;
    private readonly ISuppliersRepository _suppliersRepository;
    private readonly IUnitsRepository _unitsRepository;
    private readonly IProductsRepository _productsRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IObjectStorageService _objectStorageService;

    public UpdateSupplierOrdersService(
        ICurrentTenantContext tenantContext,
        ISupplierOrdersRepository supplierOrdersRepository,
        ISuppliersRepository suppliersRepository,
        IUnitsRepository unitsRepository,
        IProductsRepository productsRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork,
        IObjectStorageService objectStorageService)
    {
        _tenantContext = tenantContext;
        _supplierOrdersRepository = supplierOrdersRepository;
        _suppliersRepository = suppliersRepository;
        _unitsRepository = unitsRepository;
        _productsRepository = productsRepository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
        _objectStorageService = objectStorageService;
    }

    public async Task<Result<SupplierOrderDto>> ExecuteAsync(
        Guid id,
        UpdateSupplierOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;

        var pedidoSnapshot = await _supplierOrdersRepository.GetByIdAndEmpresaIdWithItensAsNoTrackingAsync(
            id,
            empresaId,
            cancellationToken);

        if (pedidoSnapshot is null)
        {
            return Result<SupplierOrderDto>.Failure("Pedido não encontrado.");
        }

        var pedido = await _supplierOrdersRepository.GetByIdAndEmpresaIdForUpdateAsync(
            id,
            empresaId,
            cancellationToken);

        if (pedido is null)
        {
            return Result<SupplierOrderDto>.Failure("Pedido não encontrado.");
        }

        var validation = await SupplierOrderRequestValidator.ValidateAsync(
            empresaId,
            request.FornecedorId,
            request.UnidadeId,
            request.TipoPedido,
            request.DataPedido,
            request.Status,
            request.Observacao,
            request.Itens,
            _suppliersRepository,
            _unitsRepository,
            _productsRepository,
            cancellationToken);

        if (validation.IsFailure)
        {
            return Result<SupplierOrderDto>.Failure(validation.Error!);
        }

        try
        {
            var dadosAnteriores = SupplierOrdersAuditSerializer.Serialize(pedidoSnapshot);
            var data = validation.Value!;

            var itensAtualizados = data.Itens
                .Select(item => ItemPedidoFornecedor.Create(
                    pedido.Id,
                    item.ProdutoId,
                    item.Quantidade,
                    item.ValorUnitario,
                    item.ValorTotal))
                .ToList();

            pedido.UpdateHeader(
                data.FornecedorId,
                data.UnidadeId,
                data.TipoPedido,
                data.DataPedido,
                data.Status,
                data.Observacao);

            pedido.ApplyItensTotal(itensAtualizados);

            await _supplierOrdersRepository.ReplaceItensAsync(pedido, itensAtualizados, cancellationToken);

            _supplierOrdersRepository.Update(pedido);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var persisted = await _supplierOrdersRepository.GetByIdAndEmpresaIdWithItensAsync(
                pedido.Id,
                empresaId,
                cancellationToken);

            await _auditLogsService.RegisterEntityChangeAsync(
                empresaId,
                _tenantContext.UsuarioId,
                nameof(PedidoFornecedor),
                pedido.Id,
                AcaoAuditoria.Editar,
                dadosAnteriores: dadosAnteriores,
                dadosNovos: SupplierOrdersAuditSerializer.Serialize(persisted ?? pedido),
                cancellationToken: cancellationToken);

            return Result<SupplierOrderDto>.Success(
                SupplierOrdersMapper.Map(persisted ?? pedido, _objectStorageService));
        }
        catch (DomainException exception)
        {
            return Result<SupplierOrderDto>.Failure(exception.Message);
        }
    }
}
