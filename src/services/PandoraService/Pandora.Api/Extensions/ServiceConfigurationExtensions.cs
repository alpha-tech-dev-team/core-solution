using CoreSolution.Shared.Results;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Pandora.Api;
using Pandora.Application.Contracts;
using Pandora.Application.Features.Audit;
using Pandora.Application.Validators;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Models;
using Pandora.Domain.Options;
using Pandora.Infrastructure.Database;
using Pandora.Infrastructure.Repositories;

namespace Pandora.Api.Extensions;

public static class ServiceConfigurationExtensions
{
    public static IServiceCollection AddServiceConfiguration(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddOpenApi();
        services.AddProblemDetails();

        var databaseOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>()
            ?? throw new InvalidOperationException($"Missing configuration section {DatabaseOptions.SectionName}.");

        ArgumentException.ThrowIfNullOrWhiteSpace(databaseOptions.ConnectionString);

        services.AddDbContext<PandoraDbContext>(options => options.UseNpgsql(databaseOptions.ConnectionString));
        services.AddScoped<IAuditRepository, AuditRepository>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<AssemblyMarker>());
        services.AddTransient<IRequestHandler<GetAuditEntriesMessage, Result<AuditEntryPage>>, GetAuditEntriesMessageHandler>();
        services.AddTransient<IValidator<GetAuditEntriesRequest>, GetAuditEntriesRequestValidator>();

        services.AddJwtAuthentication(configuration, environment);

        return services;
    }
}
