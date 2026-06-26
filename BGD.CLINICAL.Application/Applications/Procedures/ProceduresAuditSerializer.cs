using System.Text.Json;
using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Application.Applications.Procedures;

internal static class ProceduresAuditSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static string Serialize(Procedimento procedimento)
    {
        return JsonSerializer.Serialize(new
        {
            procedimento.Id,
            procedimento.EmpresaId,
            procedimento.Nome,
            procedimento.ProdutoAplicadoId,
            procedimento.Observacoes,
            procedimento.Ativo,
            Itens = procedimento.Itens.Select(item => new
            {
                item.Id,
                item.ProdutoId,
                item.Quantidade,
            }),
        }, Options);
    }
}
