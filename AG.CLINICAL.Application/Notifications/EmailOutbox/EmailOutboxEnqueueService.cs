using System.Text.Json;
using AG.CLINICAL.Application.Notifications.Abstractions;
using AG.CLINICAL.Domain.Entities;
using AG.CLINICAL.Domain.Enums;

namespace AG.CLINICAL.Application.Notifications.EmailOutbox;

public interface IEmailOutboxEnqueueService
{
    Task EnqueueAppointmentConfirmationAsync(
        Guid empresaId,
        Guid unidadeId,
        Guid agendamentoId,
        string destinatarioEmail,
        string? destinatarioNome,
        CancellationToken cancellationToken = default);

    Task EnqueueFirstAccessInvitationAsync(
        Guid empresaId,
        Guid usuarioId,
        string destinatarioEmail,
        string destinatarioNome,
        string rawToken,
        CancellationToken cancellationToken = default);
}

public sealed class EmailOutboxEnqueueService : IEmailOutboxEnqueueService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly IEmailOutboxRepository _emailOutboxRepository;

    public EmailOutboxEnqueueService(IEmailOutboxRepository emailOutboxRepository)
    {
        _emailOutboxRepository = emailOutboxRepository;
    }

    public async Task EnqueueAppointmentConfirmationAsync(
        Guid empresaId,
        Guid unidadeId,
        Guid agendamentoId,
        string destinatarioEmail,
        string? destinatarioNome,
        CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Serialize(
            new AppointmentConfirmationEmailPayload(agendamentoId),
            JsonOptions);

        var message = new OutputMessageEmail(
            empresaId,
            unidadeId,
            EmailMessageType.AppointmentConfirmation,
            destinatarioEmail,
            destinatarioNome,
            payload);

        await _emailOutboxRepository.AddAsync(message, cancellationToken);
    }

    public async Task EnqueueFirstAccessInvitationAsync(
        Guid empresaId,
        Guid usuarioId,
        string destinatarioEmail,
        string destinatarioNome,
        string rawToken,
        CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Serialize(
            new FirstAccessInvitationEmailPayload(usuarioId, rawToken),
            JsonOptions);

        var message = new OutputMessageEmail(
            empresaId,
            unidadeId: null,
            EmailMessageType.FirstAccessInvitation,
            destinatarioEmail,
            destinatarioNome,
            payload);

        await _emailOutboxRepository.AddAsync(message, cancellationToken);
    }
}
