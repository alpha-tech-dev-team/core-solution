using Atlas.App;
using MediatR;

namespace Atlas.TgBot.Extensions.DependencyInjection;

public static class ApplicationExtensions
{
public static IServiceCollection AddApplication(this IServiceCollection services)
{
    services.AddMediatR(cfg => 
        cfg.RegisterServicesFromAssembly(typeof(Atlas.App.Features.Start.StartRequest).Assembly));
        
    return services;
}}
