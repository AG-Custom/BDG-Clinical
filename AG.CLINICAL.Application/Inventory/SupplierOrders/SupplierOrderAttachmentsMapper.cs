using AG.CLINICAL.Application.Abstractions.Storage;
using AG.CLINICAL.Application.Inventory.Dtos;
using AG.CLINICAL.Domain.Entities;

namespace AG.CLINICAL.Application.Inventory.SupplierOrders;

internal static class SupplierOrderAttachmentsMapper
{
    public static SupplierOrderAttachmentDto Map(
        AnexoPedidoFornecedor anexo,
        IObjectStorageService objectStorage)
    {
        return new SupplierOrderAttachmentDto(
            anexo.Id,
            anexo.NomeArquivo,
            anexo.ContentType,
            objectStorage.BuildPublicUrl(anexo.ObjectKey),
            anexo.TamanhoBytes,
            anexo.CriadoEm);
    }

    public static IReadOnlyList<SupplierOrderAttachmentDto> Map(
        IEnumerable<AnexoPedidoFornecedor> anexos,
        IObjectStorageService objectStorage)
    {
        return anexos.Select(anexo => Map(anexo, objectStorage)).ToList();
    }
}
