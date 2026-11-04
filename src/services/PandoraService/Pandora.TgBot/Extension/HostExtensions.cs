using Microsoft.EntityFrameworkCore;
using Pandora.Domain.Entities;
using Pandora.Infrastructure.Database;

namespace Pandora.TgBot.Extension;

public static class HostExtensions
{
    private const string ClickUpItemId = "61996eaf-6ec9-4a09-96ee-622d8d28fc2e";
    private const string ClickUpResourceName = "ClickUp";
    private const string GitHubItemId = "1e1acdc1-c912-4b6d-92a4-5de9faff9ef9";
    private const string GitHubResourceName = "GitHub";
    private const string DevUsername = "service@pandora.local";
    private const long DevTelegramUserId = 705295858L;

    public static async Task ApplyMigrations(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<PandoraDbContext>().Database.MigrateAsync();
    }

    public static async Task SeedDevelopmentData(this IHost host)
    {
        var environmentName = host.Services.GetRequiredService<Microsoft.Extensions.Hosting.IHostEnvironment>().EnvironmentName;

        if (environmentName != Microsoft.Extensions.Hosting.Environments.Development)
        {
            return;
        }

        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PandoraDbContext>();
        var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(HostExtensions));

        var seedPresent = await db.SecretMetadata.AnyAsync(metadata =>
            metadata.VaultItemId == ClickUpItemId || metadata.VaultItemId == GitHubItemId);

        if (seedPresent)
        {
            logger.LogInformation("Сид-данные разработки уже присутствуют в базе");

            return;
        }

        db.SecretMetadata.AddRange(
            new SecretMetadata { VaultItemId = ClickUpItemId, ResourceName = ClickUpResourceName, Username = DevUsername },
            new SecretMetadata { VaultItemId = GitHubItemId, ResourceName = GitHubResourceName, Username = DevUsername });

        db.Permission.AddRange(
            new Permission { TelegramUserId = DevTelegramUserId, VaultItemId = ClickUpItemId, IsActive = true },
            new Permission { TelegramUserId = DevTelegramUserId, VaultItemId = GitHubItemId, IsActive = true });

        await db.SaveChangesAsync();

        logger.LogInformation("Добавлены сид-данные разработки");
    }
}
