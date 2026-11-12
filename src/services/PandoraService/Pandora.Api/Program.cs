using Pandora.Api.Constants;
using Pandora.Api.Extensions.Configuration;
using Pandora.Api.Extensions.Middleware;
using CoreSolution.Shared.Infrastructure.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.AddSharedConfiguration();

if (args.Contains(HostArguments.MigrateOnly))
{
    await builder.ApplyMigrations();

    return;
}

builder.Services.AddServiceConfiguration(builder.Configuration);

var app = builder.Build();

app.UseMiddlewareConfiguration();
app.MapRouteConfiguration();

app.Run();
