using Building.Telegram.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Pandora.Infrastructure.Database;
using Pandora.Infrastructure.DependencyInjection;
using Pandora.TgBot.Constants;
using Pandora.TgBot.Extension.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTelegramBot(builder.Configuration[ConfigurationKeys.TelegramBotToken] ?? string.Empty);
builder.Services.AddPandoraOptions(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddLocalizationResources();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddBotHandlers();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<PandoraDbContext>().Database.MigrateAsync();
}

app.Run();
