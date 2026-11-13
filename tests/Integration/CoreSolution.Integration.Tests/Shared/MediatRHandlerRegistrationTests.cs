using AtlasApplicationExtensions = Atlas.TgBot.Extensions.DependencyInjection.ApplicationExtensions;
using AtlasHandler = Atlas.Application.Features.Activation.SubmitPhoneHandler;
using CoreSolution.Shared.Application.Messages;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PandoraApplicationExtensions = Pandora.TgBot.Extension.DependencyInjection.ApplicationExtensions;
using PandoraHandler = Pandora.Application.Features.Activation.SubmitPhoneHandler;
using Xunit;

namespace CoreSolution.Integration.Tests.Shared;

public sealed class MediatRHandlerRegistrationTests
{
    private static Type? ResolveSubmitPhoneHandlerImplementation(IServiceCollection services)
    {
        var handlerServiceType = typeof(IRequestHandler<SubmitPhoneMessage, Result<WorkflowResponse>>);

        return services
            .Where(descriptor => descriptor.ServiceType == handlerServiceType)
            .Select(descriptor => descriptor.ImplementationType)
            .SingleOrDefault();
    }

    [Fact]
    public static void AddApplication_Atlas_RegistersSubmitPhoneHandlerFromApplicationAssembly()
    {
        var services = new ServiceCollection();

        AtlasApplicationExtensions.AddApplication(
            services,
            new ConfigurationBuilder().Build(),
            new TestHostEnvironment(Environments.Production));

        Assert.Equal(typeof(AtlasHandler), ResolveSubmitPhoneHandlerImplementation(services));
    }

    [Fact]
    public static void AddApplication_Pandora_RegistersSubmitPhoneHandlerFromApplicationAssembly()
    {
        var services = new ServiceCollection();

        PandoraApplicationExtensions.AddApplication(services);

        Assert.Equal(typeof(PandoraHandler), ResolveSubmitPhoneHandlerImplementation(services));
    }
}
