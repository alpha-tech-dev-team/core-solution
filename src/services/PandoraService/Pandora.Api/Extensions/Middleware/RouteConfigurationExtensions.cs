using Pandora.Api.Endpoints;

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

        return app;
    }
}
