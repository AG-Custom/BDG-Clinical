using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Identity.Abstractions;
using AG.CLINICAL.Application.Notifications.EmailOutbox;
using AG.CLINICAL.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AG.CLINICAL.Application.Identity.FirstAccess;

public interface IEmployeeFirstAccessInvitationService
{
    Task<Result<string>> StageInvitationAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default);

    Task EnqueueInvitationEmailAsync(
        Guid empresaId,
        Usuario usuario,
        string rawToken,
        CancellationToken cancellationToken = default);
}

public sealed class EmployeeFirstAccessInvitationService : IEmployeeFirstAccessInvitationService
{
    private readonly IFirstAccessInvitationsRepository _invitationsRepository;
    private readonly IEmailOutboxEnqueueService _emailOutboxEnqueueService;
    private readonly FirstAccessSettings _settings;
    private readonly ILogger<EmployeeFirstAccessInvitationService> _logger;

    public EmployeeFirstAccessInvitationService(
        IFirstAccessInvitationsRepository invitationsRepository,
        IEmailOutboxEnqueueService emailOutboxEnqueueService,
        IOptions<FirstAccessSettings> settings,
        ILogger<EmployeeFirstAccessInvitationService> logger)
    {
        _invitationsRepository = invitationsRepository;
        _emailOutboxEnqueueService = emailOutboxEnqueueService;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<Result<string>> StageInvitationAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default)
    {
        await _invitationsRepository.InvalidatePendingByUsuarioIdAsync(usuarioId, cancellationToken);

        var (convite, rawToken) = ConvitePrimeiroAcesso.Create(
            usuarioId,
            _settings.TokenExpirationHours);

        await _invitationsRepository.AddAsync(convite, cancellationToken);
        return Result<string>.Success(rawToken);
    }

    public async Task EnqueueInvitationEmailAsync(
        Guid empresaId,
        Usuario usuario,
        string rawToken,
        CancellationToken cancellationToken = default)
    {
        await _emailOutboxEnqueueService.EnqueueFirstAccessInvitationAsync(
            empresaId,
            usuario.Id,
            usuario.EmailLogin,
            usuario.Nome,
            rawToken,
            cancellationToken);

        _logger.LogInformation(
            "E-mail de primeiro acesso enfileirado para {EmailLogin} (usuário {UsuarioId}).",
            usuario.EmailLogin,
            usuario.Id);
    }
}
