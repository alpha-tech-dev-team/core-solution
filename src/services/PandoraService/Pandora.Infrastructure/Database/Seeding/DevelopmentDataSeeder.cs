using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pandora.Domain.Entities;
using Pandora.Domain.Options;

namespace Pandora.Infrastructure.Database.Seeding;

public static class DevelopmentDataSeeder
{
    public static async Task SeedIfEnabledAsync(
        IServiceProvider services,
        bool isDevelopmentEnvironment,
        CancellationToken cancellationToken = default)
    {
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DevelopmentDataSeeder));

        if (!isDevelopmentEnvironment)
        {
            return;
        }

        var options = services.GetRequiredService<IConfiguration>()
            .GetSection(SeedingOptions.SectionName)
            .Get<SeedingOptions>() ?? new SeedingOptions();

        if (!options.DevelopmentData)
        {
            logger.LogInformation("Сид-данные разработки отключены в конфигурации");

            return;
        }

        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PandoraDbContext>();

        await SeedAsync(db, logger, cancellationToken);
    }

    private static async Task SeedAsync(PandoraDbContext db, ILogger logger, CancellationToken cancellationToken)
    {
        var seedPresent = await db.SecretMetadata.AnyAsync(
            metadata => metadata.VaultItemId == DevelopmentSeedData.ClickUpItemId
                || metadata.VaultItemId == DevelopmentSeedData.GitHubItemId,
            cancellationToken);

        if (seedPresent)
        {
            logger.LogInformation("Сид-данные разработки уже присутствуют в базе");

            return;
        }

        db.SecretMetadata.AddRange(
            new SecretMetadata(
                DevelopmentSeedData.ClickUpItemId,
                DevelopmentSeedData.ClickUpResourceName,
                DevelopmentSeedData.Username),
            new SecretMetadata(
                DevelopmentSeedData.GitHubItemId,
                DevelopmentSeedData.GitHubResourceName,
                DevelopmentSeedData.Username));

        db.Permission.AddRange(
            new Permission
            {
                TelegramUserId = DevelopmentSeedData.TelegramUserId,
                VaultItemId = DevelopmentSeedData.ClickUpItemId,
                IsActive = DevelopmentSeedData.PermissionIsActive
            },
            new Permission
            {
                TelegramUserId = DevelopmentSeedData.TelegramUserId,
                VaultItemId = DevelopmentSeedData.GitHubItemId,
                IsActive = DevelopmentSeedData.PermissionIsActive
            });

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Добавлены сид-данные разработки");
    }
}
