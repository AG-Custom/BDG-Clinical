namespace AG.CLINICAL.Application.Notifications.EmailOutbox;

public sealed record AppointmentConfirmationEmailPayload(Guid AgendamentoId);

public sealed record FirstAccessInvitationEmailPayload(Guid UsuarioId, string RawToken);
