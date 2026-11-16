using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace CoreSolution.Shared.Infrastructure.Configuration;

public static class SharedConfigurationExtensions
{
    public const string SharedConfigurationFileName = "appsettings.shared.json";

    private const int SharedConfigurationPriority = 0;
    private const bool IsSharedConfigurationOptional = false;

    public static IHostApplicationBuilder AddSharedConfiguration(this IHostApplicationBuilder builder)
    {
        builder.Configuration.Sources.Insert(
            SharedConfigurationPriority,
            new JsonConfigurationSource
            {
                FileProvider = new PhysicalFileProvider(AppContext.BaseDirectory),
                Path = SharedConfigurationFileName,
                Optional = IsSharedConfigurationOptional
            });

        return builder;
    }
}
