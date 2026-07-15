using BGD.CLINICAL.Application.Applications.Dtos;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;

namespace BGD.CLINICAL.Application.Applications.PatientApplications;

internal static class PatientApplicationsMapper
{
    public static PatientApplicationDto Map(AplicacaoPaciente aplicacao)
    {
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
            MapItensConsumidos(aplicacao),
            aplicacao.CriadoEm,
            aplicacao.AtualizadoEm);
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
                movimentacao.Produto?.ControlaEstoque ?? true))
            .ToList();
    }
}
