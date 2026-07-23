using BGD.CLINICAL.Application;
using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Application.Modules.Abstractions;
using BGD.CLINICAL.Infra.Data;
using BGD.CLINICAL.Infra.Data.Services.Permissions;
using BGD.CLINICAL.Infra.ExternalApis;
using BGD.CLINICAL.WebApi.Extensions;
using BGD.CLINICAL.WebApi.Extensions.Auth;
using BGD.CLINICAL.WebApi.Extensions.Common;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfraData(builder.Configuration);
builder.Services.AddExternalApis(builder.Configuration);
builder.Services.AddAuth(builder.Configuration);
builder.Services.AddTenantContext();
var corsOrigins = FrontendCorsExtensions.ResolveAllowedOrigins(
    builder.Configuration,
    builder.Environment);
builder.Services.AddFrontendCors(builder.Configuration, builder.Environment);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddAuthorization();
builder.Services.AddHealthChecks();
builder.Services.AddOpenApi();
builder.Services.AddApiSwagger();

var app = builder.Build();

await app.MigrateDatabaseAsync();

using (var scope = app.Services.CreateScope())
{
    var catalogRepository = scope.ServiceProvider.GetRequiredService<IPermissionCatalogRepository>();
    var systemModulesRepository = scope.ServiceProvider.GetRequiredService<ISystemModulesRepository>();
    var moduleLicensesProvisioner = scope.ServiceProvider.GetRequiredService<ICompanyModuleLicensesProvisioner>();
    var measurementUnitsProvisioner = scope.ServiceProvider.GetRequiredService<ICompanyDefaultMeasurementUnitsProvisioner>();
    var productTypesProvisioner = scope.ServiceProvider.GetRequiredService<ICompanyDefaultProductTypesProvisioner>();
    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
    await PermissionSeeder.SeedAsync(catalogRepository, unitOfWork);
    await ModuleSeeder.SeedAsync(
        systemModulesRepository,
        moduleLicensesProvisioner,
        measurementUnitsProvisioner,
        productTypesProvisioner,
        unitOfWork);
}

app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "BGD Clinical API v1");
    options.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();

if (corsOrigins.Length > 0)
{
    app.UseCors(FrontendCorsExtensions.PolicyName);
}

app.UseApiExceptionHandling();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
