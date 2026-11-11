using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.FileProviders;

namespace CoreSolution.Shared.Infrastructure.Configuration;

public static class SharedConfigurationExtensions
{
    public const string SharedConfigurationFileName = "appsettings.shared.json";

    public static IConfigurationBuilder AddSharedConfiguration(this IConfigurationBuilder builder)
    {
        builder.Sources.Insert(0, new JsonConfigurationSource
        {
            FileProvider = new PhysicalFileProvider(AppContext.BaseDirectory),
            Path = SharedConfigurationFileName,
            Optional = false
        });

        return builder;
    }
}
