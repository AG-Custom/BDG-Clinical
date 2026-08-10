using BGD.CLINICAL.Application.Applications.Dtos;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;

namespace BGD.CLINICAL.Application.Applications.PatientApplications;

internal static class PatientApplicationsMapper
{
    public static PatientApplicationDto Map(AplicacaoPaciente aplicacao)
    {
        var itensConsumidos = MapItensConsumidos(aplicacao);
        var lotePrincipal = ObterLoteProdutoAplicado(aplicacao);

        return new PatientApplicationDto(
            aplicacao.Id,
            aplicacao.PacienteId,
            aplicacao.Paciente?.Nome ?? string.Empty,
            aplicacao.CompraPacienteId,
            aplicacao.ProdutoId,
            aplicacao.Produto?.Nome,
            aplicacao.ProcedimentoId,
            aplicacao.Procedimento?.Nome,
            aplicacao.FuncionarioId,
            aplicacao.Funcionario?.Nome ?? string.Empty,
            aplicacao.UnidadeId,
            aplicacao.Unidade?.Nome ?? string.Empty,
            aplicacao.DataAplicacao,
            aplicacao.QuantidadeUtilizada,
            aplicacao.Peso,
            aplicacao.Observacao,
            aplicacao.Realizado,
            aplicacao.Cancelada,
            MapSintomas(aplicacao.Sintomas),
            itensConsumidos,
            aplicacao.CriadoEm,
            aplicacao.AtualizadoEm,
            lotePrincipal?.LoteProdutoId,
            lotePrincipal?.LoteCodigo);
    }

    public static IReadOnlyList<PatientApplicationDto> Map(IReadOnlyList<AplicacaoPaciente> aplicacoes)
    {
        return aplicacoes.Select(Map).ToList();
    }

    private static IReadOnlyList<PatientApplicationSymptomDto> MapSintomas(
        IEnumerable<AplicacaoSintoma> sintomas)
    {
        return sintomas
            .Select(aplicacaoSintoma => new PatientApplicationSymptomDto(
                aplicacaoSintoma.SintomaId,
                aplicacaoSintoma.Sintoma?.Nome ?? string.Empty))
            .ToList();
    }

    private static IReadOnlyList<PatientApplicationConsumedItemDto> MapItensConsumidos(
        AplicacaoPaciente aplicacao)
    {
        return aplicacao.MovimentacoesEstoque
            .Where(movimentacao => movimentacao.Tipo == TipoMovimentacaoEstoque.Saida)
            .Select(movimentacao => new PatientApplicationConsumedItemDto(
                movimentacao.ProdutoId,
                movimentacao.Produto?.Nome ?? string.Empty,
                movimentacao.Quantidade,
                movimentacao.Produto?.ControlaEstoque ?? true,
                movimentacao.LoteProdutoId,
                movimentacao.LoteProduto?.Codigo))
            .ToList();
    }

    private static (Guid? LoteProdutoId, string? LoteCodigo)? ObterLoteProdutoAplicado(
        AplicacaoPaciente aplicacao)
    {
        if (!aplicacao.ProdutoId.HasValue)
        {
            return null;
        }

        var movimentacao = aplicacao.MovimentacoesEstoque
            .Where(item =>
                item.Tipo == TipoMovimentacaoEstoque.Saida
                && item.ProdutoId == aplicacao.ProdutoId.Value
                && item.LoteProdutoId.HasValue)
            .OrderBy(item => item.CriadoEm)
            .FirstOrDefault();

        if (movimentacao is null)
        {
            return null;
        }

        return (movimentacao.LoteProdutoId, movimentacao.LoteProduto?.Codigo);
    }
}
