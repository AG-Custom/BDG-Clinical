using AG.CLINICAL.Application.Abstractions.Persistence;
using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.Application.Abstractions.Storage;
using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Identity.Abstractions;
using AG.CLINICAL.Application.Inventory.Abstractions;
using AG.CLINICAL.Domain.Enums;

namespace AG.CLINICAL.Application.Inventory.SupplierOrders;

public interface IDeleteSupplierOrderAttachmentService
{
    Task<Result> ExecuteAsync(
        Guid pedidoId,
        Guid attachmentId,
        CancellationToken cancellationToken = default);
}

public sealed class DeleteSupplierOrderAttachmentService : IDeleteSupplierOrderAttachmentService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly ISupplierOrderAttachmentsRepository _attachmentsRepository;
    private readonly IObjectStorageService _objectStorageService;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteSupplierOrderAttachmentService(
        ICurrentTenantContext tenantContext,
        ISupplierOrderAttachmentsRepository attachmentsRepository,
        IObjectStorageService objectStorageService,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _attachmentsRepository = attachmentsRepository;
        _objectStorageService = objectStorageService;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> ExecuteAsync(
        Guid pedidoId,
        Guid attachmentId,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;
        var anexo = await _attachmentsRepository.GetByIdPedidoIdAndEmpresaIdAsync(
            attachmentId,
            pedidoId,
            empresaId,
            cancellationToken);

        if (anexo is null)
        {
            return Result.Failure("Anexo não encontrado.");
        }

        var dadosAnteriores = SupplierOrderAttachmentsAuditSerializer.Serialize(anexo);
        var objectKey = anexo.ObjectKey;

        _attachmentsRepository.Remove(anexo);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _objectStorageService.DeleteByObjectKeyAsync(objectKey, cancellationToken);

        await _auditLogsService.RegisterEntityChangeAsync(
            empresaId,
            _tenantContext.UsuarioId,
            nameof(Domain.Entities.AnexoPedidoFornecedor),
            anexo.Id,
            AcaoAuditoria.Excluir,
            dadosAnteriores: dadosAnteriores,
            cancellationToken: cancellationToken);

        return Result.Success();
    }
}
