using AG.CLINICAL.Application.Notifications;
using AG.CLINICAL.Infra.Jobs.Jobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AG.CLINICAL.Infra.Jobs;

public static class DependencyInjection
{
    public static IServiceCollection AddInfraJobs(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<EmailOutboxSettings>(configuration.GetSection("EmailOutbox"));
        services.AddHostedService<EmailOutboxWorker>();

        return services;
    }
}
