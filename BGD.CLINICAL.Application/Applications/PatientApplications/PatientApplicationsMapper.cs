using BGD.CLINICAL.Application.Applications.Dtos;
using BGD.CLINICAL.Domain.Entities;

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
            aplicacao.Produto?.Nome ?? string.Empty,
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
}
