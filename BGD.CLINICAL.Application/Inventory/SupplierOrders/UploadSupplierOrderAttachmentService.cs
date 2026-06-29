using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Abstractions.Storage;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Application.Inventory.Dtos;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using BGD.CLINICAL.Domain.Exceptions;
using Microsoft.Extensions.Options;

namespace BGD.CLINICAL.Application.Inventory.SupplierOrders;

public sealed record SupplierOrderAttachmentUpload(
    Stream Content,
    string ContentType,
    string FileName,
    long Length);

public interface IUploadSupplierOrderAttachmentService
{
    Task<Result<SupplierOrderAttachmentDto>> ExecuteAsync(
        Guid pedidoId,
        SupplierOrderAttachmentUpload upload,
        CancellationToken cancellationToken = default);
}

public sealed class UploadSupplierOrderAttachmentService : IUploadSupplierOrderAttachmentService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly ISupplierOrdersRepository _supplierOrdersRepository;
    private readonly ISupplierOrderAttachmentsRepository _attachmentsRepository;
    private readonly IObjectStorageService _objectStorageService;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CloudflareR2Settings _r2Settings;

    public UploadSupplierOrderAttachmentService(
        ICurrentTenantContext tenantContext,
        ISupplierOrdersRepository supplierOrdersRepository,
        ISupplierOrderAttachmentsRepository attachmentsRepository,
        IObjectStorageService objectStorageService,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork,
        IOptions<CloudflareR2Settings> r2Settings)
    {
        _tenantContext = tenantContext;
        _supplierOrdersRepository = supplierOrdersRepository;
        _attachmentsRepository = attachmentsRepository;
        _objectStorageService = objectStorageService;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
        _r2Settings = r2Settings.Value;
    }

    public async Task<Result<SupplierOrderAttachmentDto>> ExecuteAsync(
        Guid pedidoId,
        SupplierOrderAttachmentUpload upload,
        CancellationToken cancellationToken = default)
    {
        if (!_r2Settings.IsConfigured)
        {
            return Result<SupplierOrderAttachmentDto>.Failure("Armazenamento de arquivos não configurado.");
        }

        var validationError = ValidateUpload(upload);
        if (validationError is not null)
        {
            return Result<SupplierOrderAttachmentDto>.Failure(validationError);
        }

        var empresaId = _tenantContext.EmpresaId;
        var pedido = await _supplierOrdersRepository.GetByIdAndEmpresaIdForUpdateAsync(
            pedidoId,
            empresaId,
            cancellationToken);

        if (pedido is null)
        {
            return Result<SupplierOrderAttachmentDto>.Failure("Pedido não encontrado.");
        }

        var extension = ResolveExtension(upload.ContentType, upload.FileName);
        if (extension is null)
        {
            return Result<SupplierOrderAttachmentDto>.Failure(
                "Formato não suportado. Envie PDF, PNG, JPEG, WebP, DOC, DOCX, XLS ou XLSX.");
        }

        var objectKey = $"companies/{empresaId}/supplier-orders/{pedidoId}/{Guid.NewGuid():N}{extension}";

        try
        {
            await _objectStorageService.UploadAsync(
                new ObjectStorageUploadRequest(
                    objectKey,
                    upload.Content,
                    upload.ContentType,
                    upload.Length),
                cancellationToken);
        }
        catch (Exception)
        {
            return Result<SupplierOrderAttachmentDto>.Failure("Não foi possível enviar o arquivo. Tente novamente.");
        }

        try
        {
            var anexo = AnexoPedidoFornecedor.Create(
                pedidoId,
                empresaId,
                upload.FileName,
                upload.ContentType,
                objectKey,
                upload.Length);

            await _attachmentsRepository.AddAsync(anexo, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditLogsService.RegisterEntityChangeAsync(
                empresaId,
                _tenantContext.UsuarioId,
                nameof(AnexoPedidoFornecedor),
                anexo.Id,
                AcaoAuditoria.Criar,
                dadosNovos: SupplierOrderAttachmentsAuditSerializer.Serialize(anexo),
                cancellationToken: cancellationToken);

            return Result<SupplierOrderAttachmentDto>.Success(
                SupplierOrderAttachmentsMapper.Map(anexo, _objectStorageService));
        }
        catch (DomainException exception)
        {
            await _objectStorageService.DeleteByObjectKeyAsync(objectKey, cancellationToken);
            return Result<SupplierOrderAttachmentDto>.Failure(exception.Message);
        }
    }

    private string? ValidateUpload(SupplierOrderAttachmentUpload upload)
    {
        if (upload.Length <= 0)
        {
            return "Selecione um arquivo.";
        }

        if (upload.Length > _r2Settings.MaxAttachmentSizeBytes)
        {
            var maxMb = _r2Settings.MaxAttachmentSizeBytes / (1024 * 1024);
            return $"O arquivo deve ter no máximo {maxMb} MB.";
        }

        var contentType = upload.ContentType?.Trim().ToLowerInvariant() ?? string.Empty;

        if (!_r2Settings.AllowedAttachmentContentTypes.Contains(contentType))
        {
            return "Formato não suportado. Envie PDF, PNG, JPEG, WebP, DOC, DOCX, XLS ou XLSX.";
        }

        if (ResolveExtension(contentType, upload.FileName) is null)
        {
            return "Formato não suportado. Envie PDF, PNG, JPEG, WebP, DOC, DOCX, XLS ou XLSX.";
        }

        return null;
    }

    private static string? ResolveExtension(string contentType, string fileName)
    {
        return contentType.Trim().ToLowerInvariant() switch
        {
            "application/pdf" => ".pdf",
            "image/png" => ".png",
            "image/jpeg" => ".jpg",
            "image/webp" => ".webp",
            "application/msword" => ".doc",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => ".docx",
            "application/vnd.ms-excel" => ".xls",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" => ".xlsx",
            _ => ResolveExtensionFromFileName(fileName),
        };
    }

    private static string? ResolveExtensionFromFileName(string fileName)
    {
        var extension = Path.GetExtension(fileName).Trim().ToLowerInvariant();

        return extension switch
        {
            ".pdf" => ".pdf",
            ".png" => ".png",
            ".jpg" or ".jpeg" => ".jpg",
            ".webp" => ".webp",
            ".doc" => ".doc",
            ".docx" => ".docx",
            ".xls" => ".xls",
            ".xlsx" => ".xlsx",
            _ => null,
        };
    }
}
