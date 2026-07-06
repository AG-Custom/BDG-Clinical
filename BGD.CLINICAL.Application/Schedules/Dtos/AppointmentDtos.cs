namespace BGD.CLINICAL.Application.Schedules.Dtos;

public sealed record AppointmentProcedureDto(Guid Id, string Nome);

public sealed record AppointmentDto(
    Guid Id,
    Guid UnidadeId,
    string UnidadeNome,
    Guid PacienteId,
    string PacienteNome,
    Guid FuncionarioId,
    string FuncionarioNome,
    Guid? ProcedimentoId,
    string? ProcedimentoNome,
    IReadOnlyList<AppointmentProcedureDto> Procedimentos,
    string Tipo,
    string Status,
    DateTime DataInicio,
    DateTime DataFim,
    string? Observacao,
    Guid CriadoPorId,
    string CriadoPorNome,
    Guid? CanceladoPorId,
    string? MotivoCancelamento,
    bool ExcecaoHorario,
    Guid? AplicacaoPacienteId,
    IReadOnlyList<Guid> AplicacaoPacienteIds,
    DateTime CriadoEm,
    DateTime? AtualizadoEm);

public sealed record CreateAppointmentRequest(
    Guid UnidadeId,
    Guid PacienteId,
    Guid FuncionarioId,
    string Tipo,
    DateTime DataInicio,
    DateTime DataFim,
    Guid? ProcedimentoId = null,
    IReadOnlyList<Guid>? ProcedimentoIds = null,
    string? Observacao = null,
    int ExcecaoHorario = 0);

public sealed record UpdateAppointmentRequest(
    Guid UnidadeId,
    Guid PacienteId,
    Guid FuncionarioId,
    string Tipo,
    DateTime DataInicio,
    DateTime DataFim,
    Guid? ProcedimentoId = null,
    IReadOnlyList<Guid>? ProcedimentoIds = null,
    string? Observacao = null,
    int ExcecaoHorario = 0);

public sealed record CancelAppointmentRequest(string Motivo);

public sealed record CompleteAppointmentProcedureRequest(
    Guid ProcedimentoId,
    decimal? QuantidadeUtilizada = null,
    decimal? Peso = null);

public sealed record CompleteAppointmentRequest(
    decimal? QuantidadeUtilizada = null,
    decimal? Peso = null,
    IReadOnlyList<CompleteAppointmentProcedureRequest>? Procedimentos = null);
