using AG.CLINICAL.Application.Applications.Dtos;
using AG.CLINICAL.Domain.Entities;

namespace AG.CLINICAL.Application.Applications.Procedures;

internal static class ProceduresMapper
{
    public static ProcedureDto Map(Procedimento procedimento)
    {
        return new ProcedureDto(
            procedimento.Id,
            procedimento.Nome,
            procedimento.ProdutoAplicadoId,
            procedimento.ProdutoAplicado?.Nome,
            procedimento.Observacoes,
            procedimento.Ativo,
            MapItens(procedimento.Itens),
            procedimento.CriadoEm,
            procedimento.AtualizadoEm);
    }

    public static IReadOnlyList<ProcedureDto> Map(IReadOnlyList<Procedimento> procedimentos)
    {
        return procedimentos.Select(Map).ToList();
    }

    private static IReadOnlyList<ProcedureItemDto> MapItens(IEnumerable<ItemProcedimento> itens)
    {
        return itens
            .Select(item => new ProcedureItemDto(
                item.Id,
                item.ProdutoId,
                item.Produto?.Nome ?? string.Empty,
                item.Quantidade))
            .ToList();
    }
}
