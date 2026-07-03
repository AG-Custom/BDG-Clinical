using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Application.Schedules.Appointments;

internal static class AppointmentsMapper
{
    public static Dtos.AppointmentDto Map(Agendamento agendamento)
    {
        return new Dtos.AppointmentDto(
            agendamento.Id,
            agendamento.UnidadeId,
            agendamento.Unidade?.Nome ?? string.Empty,
            agendamento.PacienteId,
            agendamento.Paciente?.Nome ?? string.Empty,
            agendamento.FuncionarioId,
            agendamento.Funcionario?.Nome ?? string.Empty,
            agendamento.ProcedimentoId,
            agendamento.Procedimento?.Nome,
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
            agendamento.AplicacaoPaciente?.Id,
            agendamento.CriadoEm,
            agendamento.AtualizadoEm);
    }
}
