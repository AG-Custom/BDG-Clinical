namespace AG.CLINICAL.Application.Notifications;

public sealed class EmailOutboxSettings
{
    public int PollingIntervalSeconds { get; set; } = 5;

    public int BatchSize { get; set; } = 10;
}
