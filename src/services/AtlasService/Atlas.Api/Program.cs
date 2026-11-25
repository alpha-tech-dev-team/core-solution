using Atlas.Api.Constants;
using Atlas.Api.Extensions.Configuration;
using Atlas.Api.Extensions.Middleware;

var builder = WebApplication.CreateBuilder(args);

if (args.Contains(HostArguments.MigrateOnly))
{
    await builder.ApplyMigrations();

    return;
}

builder.Services.AddServiceConfiguration(builder.Configuration, builder.Environment);

var app = builder.Build();

app.UseMiddlewareConfiguration();
app.MapRouteConfiguration();

app.Run();
