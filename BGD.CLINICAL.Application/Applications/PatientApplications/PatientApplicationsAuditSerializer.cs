using System.Text.Json;
using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Application.Applications.PatientApplications;

internal static class PatientApplicationsAuditSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static string Serialize(AplicacaoPaciente aplicacao)
    {
        return JsonSerializer.Serialize(new
        {
            aplicacao.Id,
            aplicacao.EmpresaId,
            aplicacao.PacienteId,
            aplicacao.CompraPacienteId,
            aplicacao.ProdutoId,
            aplicacao.FuncionarioId,
            aplicacao.UnidadeId,
            aplicacao.DataAplicacao,
            aplicacao.QuantidadeUtilizada,
            aplicacao.Peso,
            aplicacao.Observacao,
            aplicacao.Realizado,
            aplicacao.Cancelada,
            Sintomas = aplicacao.Sintomas.Select(s => s.SintomaId),
        }, Options);
    }

    public static string Serialize(MovimentacaoEstoque movimentacao)
    {
        return JsonSerializer.Serialize(new
        {
            movimentacao.Id,
            movimentacao.EmpresaId,
            movimentacao.UnidadeId,
            movimentacao.ProdutoId,
            Tipo = movimentacao.Tipo.ToString(),
            movimentacao.Quantidade,
            movimentacao.Data,
            movimentacao.Origem,
            movimentacao.AplicacaoPacienteId,
            movimentacao.FuncionarioId,
        }, Options);
    }
}
