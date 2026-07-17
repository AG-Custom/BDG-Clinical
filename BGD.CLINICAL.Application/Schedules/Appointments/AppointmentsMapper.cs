using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Application.Schedules.Appointments;

internal static class AppointmentsMapper
{
    public static Dtos.AppointmentDto Map(Agendamento agendamento)
    {
        var procedimentos = agendamento.ProcedimentosVinculados
            .OrderBy(item => item.CriadoEm)
            .Select(item => new Dtos.AppointmentProcedureDto(
                item.ProcedimentoId,
                item.Procedimento?.Nome ?? string.Empty))
            .ToList();

        if (procedimentos.Count == 0 && agendamento.ProcedimentoId.HasValue)
        {
            procedimentos.Add(new Dtos.AppointmentProcedureDto(
                agendamento.ProcedimentoId.Value,
                agendamento.Procedimento?.Nome ?? string.Empty));
        }

        var aplicacoesAtivas = agendamento.AplicacoesPaciente
            .Where(aplicacao => !aplicacao.Cancelada)
            .Select(aplicacao => aplicacao.Id)
            .ToList();

        var primeiroProcedimento = procedimentos.FirstOrDefault();

        return new Dtos.AppointmentDto(
            agendamento.Id,
            agendamento.UnidadeId,
            agendamento.Unidade?.Nome ?? string.Empty,
            agendamento.PacienteId,
            agendamento.Paciente?.Nome ?? string.Empty,
            agendamento.FuncionarioId,
            agendamento.Funcionario?.Nome ?? string.Empty,
            primeiroProcedimento?.Id,
            primeiroProcedimento?.Nome,
            agendamento.CompraPacienteId,
            procedimentos,
            agendamento.Tipo.ToString(),
            agendamento.Status.ToString(),
            agendamento.DataInicio,
            agendamento.DataFim,
            agendamento.Observacao,
            agendamento.CriadoPorId,
            agendamento.CriadoPor?.Nome ?? string.Empty,
            agendamento.CanceladoPorId,
            agendamento.MotivoCancelamento,
            agendamento.ExcecaoHorario,
            aplicacoesAtivas.FirstOrDefault(),
            aplicacoesAtivas,
            agendamento.CriadoEm,
            agendamento.AtualizadoEm);
    }
}
