using AG.CLINICAL.Application.Notifications.EmailOutbox;
using AG.CLINICAL.Application.Notifications;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AG.CLINICAL.Infra.Jobs.Jobs;

public sealed class EmailOutboxWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly EmailOutboxSettings _settings;
    private readonly ILogger<EmailOutboxWorker> _logger;

    public EmailOutboxWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<EmailOutboxSettings> settings,
        ILogger<EmailOutboxWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Worker de e-mail iniciado. Intervalo: {Interval}s, lote: {BatchSize}.",
            _settings.PollingIntervalSeconds,
            _settings.BatchSize);

        var pollingInterval = TimeSpan.FromSeconds(Math.Max(1, _settings.PollingIntervalSeconds));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<IProcessEmailOutboxService>();
                var processed = await processor.ProcessBatchAsync(stoppingToken);

                if (processed == 0)
                {
                    await Task.Delay(pollingInterval, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Erro no worker de e-mail. Nova tentativa em {Interval}s.", pollingInterval.TotalSeconds);
                await Task.Delay(pollingInterval, stoppingToken);
            }
        }
    }
}
