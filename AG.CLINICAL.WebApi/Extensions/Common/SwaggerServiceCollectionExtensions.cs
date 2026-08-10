using Microsoft.OpenApi;

namespace AG.CLINICAL.WebApi.Extensions.Common;

public static class SwaggerServiceCollectionExtensions
{
    public static IServiceCollection AddApiSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "AG Clinical API",
                Version = "v1",
            });

            options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "JWT obtido no login. Informe apenas o token (sem o prefixo Bearer).",
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("bearer", document)] = [],
            });
        });

        return services;
    }
}
