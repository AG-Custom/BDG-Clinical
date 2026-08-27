namespace AG.CLINICAL.Application.Common;

/// <summary>
/// Centraliza a conversao entre os instantes armazenados em UTC e o horario
/// civil usado pelas clinicas no Brasil.
/// </summary>
public static class BrazilTime
{
    public const string IanaTimeZoneId = "America/Sao_Paulo";
    public const string WindowsTimeZoneId = "E. South America Standard Time";

    private static readonly TimeZoneInfo TimeZone = ResolveTimeZone();

    public static DateTime FromUtc(DateTime value)
    {
        var utc = value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

        return TimeZoneInfo.ConvertTimeFromUtc(utc, TimeZone);
    }

    private static TimeZoneInfo ResolveTimeZone()
    {
        var preferredId = OperatingSystem.IsWindows() ? WindowsTimeZoneId : IanaTimeZoneId;
        var fallbackId = OperatingSystem.IsWindows() ? IanaTimeZoneId : WindowsTimeZoneId;

        if (TimeZoneInfo.TryFindSystemTimeZoneById(preferredId, out var preferredTimeZone))
        {
            return preferredTimeZone;
        }

        if (TimeZoneInfo.TryFindSystemTimeZoneById(fallbackId, out var fallbackTimeZone))
        {
            return fallbackTimeZone;
        }

        return TimeZoneInfo.CreateCustomTimeZone(
            IanaTimeZoneId,
            TimeSpan.FromHours(-3),
            "Brasilia Standard Time",
            "Brasilia Standard Time");
    }
}
