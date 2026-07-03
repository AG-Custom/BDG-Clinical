using System.Text.Json;
using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Application.Schedules.Appointments;

internal static class AppointmentsAuditSerializer
{
    public static string Serialize(Agendamento agendamento)
    {
        return JsonSerializer.Serialize(new
        {
            agendamento.Id,
            agendamento.UnidadeId,
            agendamento.PacienteId,
            agendamento.FuncionarioId,
            agendamento.ProcedimentoId,
            Tipo = agendamento.Tipo.ToString(),
            Status = agendamento.Status.ToString(),
            agendamento.DataInicio,
            agendamento.DataFim,
            agendamento.Observacao,
            agendamento.ExcecaoHorario,
            agendamento.CanceladoPorId,
            agendamento.MotivoCancelamento
        });
    }
}
