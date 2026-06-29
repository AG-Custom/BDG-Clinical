using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Abstractions.Storage;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Application.Inventory.Dtos;

namespace BGD.CLINICAL.Application.Inventory.SupplierOrders;

public interface IListSupplierOrderAttachmentsService
{
    Task<Result<IReadOnlyList<SupplierOrderAttachmentDto>>> ExecuteAsync(
        Guid pedidoId,
        CancellationToken cancellationToken = default);
}

public sealed class ListSupplierOrderAttachmentsService : IListSupplierOrderAttachmentsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly ISupplierOrdersRepository _supplierOrdersRepository;
    private readonly ISupplierOrderAttachmentsRepository _attachmentsRepository;
    private readonly IObjectStorageService _objectStorageService;

    public ListSupplierOrderAttachmentsService(
        ICurrentTenantContext tenantContext,
        ISupplierOrdersRepository supplierOrdersRepository,
        ISupplierOrderAttachmentsRepository attachmentsRepository,
        IObjectStorageService objectStorageService)
    {
        _tenantContext = tenantContext;
        _supplierOrdersRepository = supplierOrdersRepository;
        _attachmentsRepository = attachmentsRepository;
        _objectStorageService = objectStorageService;
    }

    public async Task<Result<IReadOnlyList<SupplierOrderAttachmentDto>>> ExecuteAsync(
        Guid pedidoId,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;
        var pedido = await _supplierOrdersRepository.GetByIdAndEmpresaIdWithItensAsNoTrackingAsync(
            pedidoId,
            empresaId,
            cancellationToken);

        if (pedido is null)
        {
            return Result<IReadOnlyList<SupplierOrderAttachmentDto>>.Failure("Pedido não encontrado.");
        }

        var anexos = await _attachmentsRepository.ListByPedidoIdAndEmpresaIdAsync(
            pedidoId,
            empresaId,
            cancellationToken);

        return Result<IReadOnlyList<SupplierOrderAttachmentDto>>.Success(
            SupplierOrderAttachmentsMapper.Map(anexos, _objectStorageService));
    }
}
