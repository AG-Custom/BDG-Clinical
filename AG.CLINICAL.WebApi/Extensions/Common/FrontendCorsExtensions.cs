namespace AG.CLINICAL.WebApi.Extensions.Common;

public sealed class CorsSettings
{
    public string[] AllowedOrigins { get; set; } = [];
}

public static class FrontendCorsExtensions
{
    public const string PolicyName = "Frontend";

    private static readonly string[] DevelopmentOrigins =
    [
        "http://localhost:5173",
        "https://localhost:5173",
        "http://127.0.0.1:5173",
        "https://127.0.0.1:5173",
    ];

    public static string[] ResolveAllowedOrigins(
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var settings = configuration.GetSection("Cors").Get<CorsSettings>() ?? new CorsSettings();
        var origins = new HashSet<string>(settings.AllowedOrigins, StringComparer.OrdinalIgnoreCase);

        if (environment.IsDevelopment())
        {
            foreach (var origin in DevelopmentOrigins)
                origins.Add(origin);
        }

        return origins.ToArray();
    }

    public static IServiceCollection AddFrontendCors(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var origins = ResolveAllowedOrigins(configuration, environment);

        if (origins.Length == 0)
            return services;

        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
            {
                policy
                    .WithOrigins(origins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }
}
