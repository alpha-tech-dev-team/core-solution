using Pandora.Api.Endpoints;
using Pandora.Api.Hubs;

namespace Pandora.Api.Extensions.Middleware;

public static class RouteConfigurationExtensions
{
    public static WebApplication MapRouteConfiguration(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.MapSecretsEndpoints();
        app.MapAuditEndpoints();
        app.MapHub<AuditHub>(AuditHub.Path);
        app.MapEmployeeSecretsEndpoints();

        return app;
    }
}
