using BGD.CLINICAL.Application.Inventory.Dtos;
using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Application.Inventory.ProductTypes;

internal static class ProductTypesMapper
{
    public static ProductTypeDto Map(TipoProduto tipoProduto)
    {
        return new ProductTypeDto(
            tipoProduto.Id,
            tipoProduto.Nome,
            tipoProduto.Ativo,
            tipoProduto.CriadoEm,
            tipoProduto.AtualizadoEm);
    }

    public static IReadOnlyList<ProductTypeDto> Map(IReadOnlyList<TipoProduto> tiposProduto)
    {
        return tiposProduto.Select(Map).ToList();
    }
}
