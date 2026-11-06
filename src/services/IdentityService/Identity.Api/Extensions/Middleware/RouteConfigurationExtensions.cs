using Identity.Api.Endpoints;

namespace Identity.Api.Extensions.Middleware;

public static class RouteConfigurationExtensions
{
    public static WebApplication MapRouteConfiguration(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.MapIdentityEndpoints();
        app.MapAdminEndpoints();

        return app;
    }
}
