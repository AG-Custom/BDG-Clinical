using BGD.CLINICAL.Application.Abstractions.Storage;
using BGD.CLINICAL.Application.Inventory.Dtos;
using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Application.Inventory.SupplierOrders;

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
