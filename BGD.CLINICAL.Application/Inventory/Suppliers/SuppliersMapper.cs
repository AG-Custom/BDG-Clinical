using BGD.CLINICAL.Application.Inventory.Dtos;
using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Application.Inventory.Suppliers;

internal static class SuppliersMapper
{
    public static SupplierDto Map(Fornecedor fornecedor)
    {
        return new SupplierDto(
            fornecedor.Id,
            fornecedor.Nome,
            fornecedor.Cnpj,
            fornecedor.Telefone,
            fornecedor.Email,
            fornecedor.Ativo,
            fornecedor.CriadoEm,
            fornecedor.AtualizadoEm);
    }

    public static IReadOnlyList<SupplierDto> Map(IReadOnlyList<Fornecedor> fornecedores)
    {
        return fornecedores.Select(Map).ToList();
    }
}
