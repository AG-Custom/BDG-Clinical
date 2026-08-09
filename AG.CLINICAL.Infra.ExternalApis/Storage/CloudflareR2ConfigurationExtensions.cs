using AG.CLINICAL.Application.Abstractions.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AG.CLINICAL.Infra.ExternalApis.Storage;

internal static class CloudflareR2ConfigurationExtensions
{
    public static IServiceCollection ConfigureCloudflareR2(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<CloudflareR2Settings>(options =>
        {
            configuration.GetSection("CloudflareR2").Bind(options);

            options.AccountId = FirstNonEmpty(
                options.AccountId,
                configuration["CLOUDFLARE_R2_ACCOUNT_ID"]);

            options.BucketName = FirstNonEmpty(
                options.BucketName,
                configuration["CLOUDFLARE_R2_BUCKET_NAME"]);

            options.AccessKeyId = FirstNonEmpty(
                options.AccessKeyId,
                configuration["CLOUDFLARE_R2_ACCESS_KEY_ID"]);

            options.SecretAccessKey = FirstNonEmpty(
                options.SecretAccessKey,
                configuration["CLOUDFLARE_R2_SECRET_ACCESS_KEY"]);

            options.ServiceUrl = FirstNonEmpty(
                options.ServiceUrl,
                configuration["CLOUDFLARE_R2_ENDPOINT"]);

            options.PublicBaseUrl = FirstNonEmpty(
                options.PublicBaseUrl,
                configuration["CLOUDFLARE_R2_PUBLIC_URL"]);

            if (string.IsNullOrWhiteSpace(options.ServiceUrl)
                && !string.IsNullOrWhiteSpace(options.AccountId))
            {
                options.ServiceUrl =
                    $"https://{options.AccountId.Trim()}.r2.cloudflarestorage.com";
            }
        });

        return services;
    }

    private static string FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return string.Empty;
    }
}
