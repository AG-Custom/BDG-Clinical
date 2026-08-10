using AG.CLINICAL.Application;
using AG.CLINICAL.Infra.Data;
using AG.CLINICAL.Infra.ExternalApis;
using AG.CLINICAL.Infra.Jobs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationJobs();
builder.Services.AddInfraData(builder.Configuration);
builder.Services.AddExternalApis(builder.Configuration);
builder.Services.AddInfraJobs(builder.Configuration);

builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health");

app.Run();
