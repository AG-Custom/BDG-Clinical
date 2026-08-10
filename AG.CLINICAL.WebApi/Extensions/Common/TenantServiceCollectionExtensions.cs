using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.WebApi.Infrastructure.Security;

namespace AG.CLINICAL.WebApi.Extensions.Common;

public static class TenantServiceCollectionExtensions
{
    public static IServiceCollection AddTenantContext(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentTenantContext, CurrentTenantContext>();

        return services;
    }
}
