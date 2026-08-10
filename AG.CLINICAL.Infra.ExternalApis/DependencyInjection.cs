using AG.CLINICAL.Application.Abstractions.Notifications;
using AG.CLINICAL.Application.Abstractions.Storage;
using AG.CLINICAL.Application.Identity;
using AG.CLINICAL.Infra.ExternalApis.Clients;
using AG.CLINICAL.Infra.ExternalApis.Email;
using AG.CLINICAL.Infra.ExternalApis.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AG.CLINICAL.Infra.ExternalApis;

public static class DependencyInjection
{
    public static IServiceCollection AddExternalApis(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpClient<ExternalApiClient>();

        services.Configure<SmtpSettings>(configuration.GetSection("Smtp"));
        services.Configure<FirstAccessSettings>(configuration.GetSection("FirstAccess"));
        services.ConfigureCloudflareR2(configuration);

        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddScoped<IObjectStorageService, CloudflareR2ObjectStorageService>();

        return services;
    }
}
