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

public interface ICancelSupplierOrdersService
{
    Task<Result<SupplierOrderDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

public sealed class CancelSupplierOrdersService : ICancelSupplierOrdersService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly ISupplierOrdersRepository _supplierOrdersRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IObjectStorageService _objectStorageService;

    public CancelSupplierOrdersService(
        ICurrentTenantContext tenantContext,
        ISupplierOrdersRepository supplierOrdersRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork,
        IObjectStorageService objectStorageService)
    {
        _tenantContext = tenantContext;
        _supplierOrdersRepository = supplierOrdersRepository;
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

            pedido.Cancel();
            _supplierOrdersRepository.Update(pedido);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditLogsService.RegisterEntityChangeAsync(
                empresaId,
                _tenantContext.UsuarioId,
                nameof(PedidoFornecedor),
                pedido.Id,
                AcaoAuditoria.Cancelar,
                dadosAnteriores: dadosAnteriores,
                dadosNovos: SupplierOrdersAuditSerializer.Serialize(pedido),
                cancellationToken: cancellationToken);

            return Result<SupplierOrderDto>.Success(
                SupplierOrdersMapper.Map(pedido, _objectStorageService));
        }
        catch (DomainException exception)
        {
            return Result<SupplierOrderDto>.Failure(exception.Message);
        }
    }
}
