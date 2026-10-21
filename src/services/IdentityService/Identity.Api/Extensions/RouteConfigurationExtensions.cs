using Identity.Api.Endpoints;

namespace Identity.Api.Extensions;

public static class RouteConfigurationExtensions
{
    public static WebApplication MapRouteConfiguration(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.MapIdentityEndpoints();

        return app;
    }
}
