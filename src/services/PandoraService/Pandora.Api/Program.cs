using Pandora.Api.Constants;
using Pandora.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

if (args.Contains(HostArguments.MigrateOnly))
{
    builder.Services.AddDatabaseConfiguration(builder.Configuration);

    var migrator = builder.Build();
    await migrator.ApplyMigrations();

    return;
}

builder.Services.AddServiceConfiguration(builder.Configuration);

var app = builder.Build();

app.UseMiddlewareConfiguration();
app.MapRouteConfiguration();

app.Run();
