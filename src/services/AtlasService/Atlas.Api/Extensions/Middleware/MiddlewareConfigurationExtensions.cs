using Serilog;

namespace Atlas.Api.Extensions.Middleware;

public static class MiddlewareConfigurationExtensions
{
    public static WebApplication UseMiddlewareConfiguration(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        app.UseExceptionHandler();

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}
