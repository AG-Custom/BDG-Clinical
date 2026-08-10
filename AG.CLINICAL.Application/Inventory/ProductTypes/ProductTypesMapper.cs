using AG.CLINICAL.Application.Inventory.Dtos;
using AG.CLINICAL.Domain.Entities;

namespace AG.CLINICAL.Application.Inventory.ProductTypes;

internal static class ProductTypesMapper
{
    public static ProductTypeDto Map(TipoProduto tipoProduto)
    {
        return new ProductTypeDto(
            tipoProduto.Id,
            tipoProduto.Nome,
            tipoProduto.Codigo,
            tipoProduto.Ativo,
            tipoProduto.CriadoEm,
            tipoProduto.AtualizadoEm);
    }

    public static IReadOnlyList<ProductTypeDto> Map(IReadOnlyList<TipoProduto> tiposProduto)
    {
        return tiposProduto.Select(Map).ToList();
    }
}
