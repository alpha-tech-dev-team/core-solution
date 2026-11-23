using Atlas.Api.Endpoints;

namespace Atlas.Api.Extensions.Middleware;

public static class RouteConfigurationExtensions
{
    public static WebApplication MapRouteConfiguration(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.MapIncidentsEndpoints();
        app.MapRoomsEndpoints();

        return app;
    }
}
