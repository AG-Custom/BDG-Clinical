using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.WebApi.Infrastructure.Security;

namespace BGD.CLINICAL.WebApi.Extensions.Common;

public static class TenantServiceCollectionExtensions
{
    public static IServiceCollection AddTenantContext(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentTenantContext, CurrentTenantContext>();

        return services;
    }
}
