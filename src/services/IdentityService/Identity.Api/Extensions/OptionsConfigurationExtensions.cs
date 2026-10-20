using Identity.Core.Options;

namespace Identity.Infrastructure.Options.Extensions;

public static class OptionsConfigurationExtensions
{
    public static IServiceCollection ConfigureOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));

        return services;
    }
}