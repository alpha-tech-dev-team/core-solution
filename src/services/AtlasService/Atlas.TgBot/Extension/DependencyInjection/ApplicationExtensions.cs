using Atlas.App.Features.Text;
using MediatR;

namespace Atlas.TgBot.Extension.DependencyInjection;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<TextCommand>());

        return services;
    }
}
