using System.Text.Json;
using AG.CLINICAL.Application.Abstractions.Notifications;
using AG.CLINICAL.Application.Abstractions.Persistence;
using AG.CLINICAL.Application.Identity;
using AG.CLINICAL.Application.Identity.Abstractions;
using AG.CLINICAL.Application.Identity.FirstAccess;
using AG.CLINICAL.Application.Notifications.Abstractions;
using AG.CLINICAL.Application.Schedules.Abstractions;
using AG.CLINICAL.Application.Schedules.Appointments;
using AG.CLINICAL.Domain.Entities;
using AG.CLINICAL.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AG.CLINICAL.Application.Notifications.EmailOutbox;

public interface IProcessEmailOutboxService
{
    Task<int> ProcessBatchAsync(CancellationToken cancellationToken = default);
}

public sealed class ProcessEmailOutboxService : IProcessEmailOutboxService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly IEmailOutboxRepository _emailOutboxRepository;
    private readonly IAppointmentsRepository _appointmentsRepository;
    private readonly IUsersRepository _usersRepository;
    private readonly IRepository<Empresa> _empresaRepository;
    private readonly IEmailSender _emailSender;
    private readonly FirstAccessSettings _firstAccessSettings;
    private readonly EmailOutboxSettings _outboxSettings;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProcessEmailOutboxService> _logger;

    public ProcessEmailOutboxService(
        IEmailOutboxRepository emailOutboxRepository,
        IAppointmentsRepository appointmentsRepository,
        IUsersRepository usersRepository,
        IRepository<Empresa> empresaRepository,
        IEmailSender emailSender,
        IOptions<FirstAccessSettings> firstAccessSettings,
        IOptions<EmailOutboxSettings> outboxSettings,
        IUnitOfWork unitOfWork,
        ILogger<ProcessEmailOutboxService> logger)
    {
        _emailOutboxRepository = emailOutboxRepository;
        _appointmentsRepository = appointmentsRepository;
        _usersRepository = usersRepository;
        _empresaRepository = empresaRepository;
        _emailSender = emailSender;
        _firstAccessSettings = firstAccessSettings.Value;
        _outboxSettings = outboxSettings.Value;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<int> ProcessBatchAsync(CancellationToken cancellationToken = default)
    {
        var batch = await _emailOutboxRepository.GetPendingBatchAsync(
            _outboxSettings.BatchSize,
            cancellationToken);

        if (batch.Count == 0)
        {
            return 0;
        }

        foreach (var message in batch)
        {
            message.MarkAsProcessing();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var processed = 0;

        foreach (var message in batch)
        {
            try
            {
                await DispatchAsync(message, cancellationToken);
                message.MarkAsSent();
                processed++;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Falha ao processar mensagem de e-mail {MessageId} (tipo {Tipo}).",
                    message.Id,
                    message.Tipo);

                message.MarkAsFailed(exception.Message);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return processed;
    }

    private async Task DispatchAsync(OutputMessageEmail message, CancellationToken cancellationToken)
    {
        switch (message.Tipo)
        {
            case EmailMessageType.AppointmentConfirmation:
                await SendAppointmentConfirmationAsync(message, cancellationToken);
                break;
            case EmailMessageType.FirstAccessInvitation:
                await SendFirstAccessInvitationAsync(message, cancellationToken);
                break;
            default:
                throw new InvalidOperationException($"Tipo de e-mail não suportado: {message.Tipo}.");
        }
    }

    private async Task SendAppointmentConfirmationAsync(
        OutputMessageEmail message,
        CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Deserialize<AppointmentConfirmationEmailPayload>(message.PayloadJson, JsonOptions)
            ?? throw new InvalidOperationException("Payload de confirmação de agendamento inválido.");

        var agendamento = await _appointmentsRepository.GetByIdAndEmpresaIdWithDetailsAsync(
            payload.AgendamentoId,
            message.EmpresaId,
            cancellationToken);

        if (agendamento is null)
        {
            throw new InvalidOperationException("Agendamento não encontrado para envio de e-mail.");
        }

        var subject = $"Agendamento confirmado — {agendamento.Empresa?.Nome ?? "Clínica"}";
        var htmlBody = AppointmentConfirmationEmailTemplate.Build(
            agendamento,
            _firstAccessSettings.PlatformLogoUrl);

        await _emailSender.SendAsync(message.DestinatarioEmail, subject, htmlBody, cancellationToken);
    }

    private async Task SendFirstAccessInvitationAsync(
        OutputMessageEmail message,
        CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Deserialize<FirstAccessInvitationEmailPayload>(message.PayloadJson, JsonOptions)
            ?? throw new InvalidOperationException("Payload de primeiro acesso inválido.");

        if (string.IsNullOrWhiteSpace(_firstAccessSettings.FrontendBaseUrl))
        {
            throw new InvalidOperationException("URL do frontend não configurada para primeiro acesso.");
        }

        var usuario = await _usersRepository.GetByIdAsync(payload.UsuarioId, cancellationToken);
        if (usuario is null || !usuario.Ativo)
        {
            throw new InvalidOperationException("Usuário não encontrado para envio de convite.");
        }

        var empresa = await _empresaRepository.GetByIdAsync(message.EmpresaId, cancellationToken);
        if (empresa is null)
        {
            throw new InvalidOperationException("Empresa não encontrada para envio de convite.");
        }

        var firstAccessUrl = BuildFirstAccessUrl(payload.RawToken);
        var subject = $"Acesso à plataforma — {empresa.Nome}";
        var htmlBody = FirstAccessInvitationEmailTemplate.Build(
            usuario.Nome,
            empresa.Nome,
            firstAccessUrl,
            _firstAccessSettings.TokenExpirationHours,
            empresa.Logo,
            _firstAccessSettings.PlatformLogoUrl);

        await _emailSender.SendAsync(message.DestinatarioEmail, subject, htmlBody, cancellationToken);
    }

    private string BuildFirstAccessUrl(string rawToken)
    {
        var baseUrl = _firstAccessSettings.FrontendBaseUrl.TrimEnd('/');
        var path = string.IsNullOrWhiteSpace(_firstAccessSettings.Path) ? "/primeiro-acesso" : _firstAccessSettings.Path;

        if (!path.StartsWith('/'))
        {
            path = "/" + path;
        }

        return $"{baseUrl}{path}?token={Uri.EscapeDataString(rawToken)}";
    }
}
