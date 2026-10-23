using Atlas.Infrastructure.Options;
using Atlas.Infrastructure.Access;
using Atlas.Core.Interfaces;
using Atlas.TgBot.Extensions.DependencyInjection;
using Building.Telegram.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

var telegramOptions = builder.Configuration.GetTelegramOptions();

builder.Services.Configure<AccessOptions>(
    builder.Configuration.GetSection(AccessOptions.SectionName));

builder.Services.AddSingleton<IAccessService, AccessService>();

builder.Services.AddTelegramBot(telegramOptions.BotToken);
builder.Services.AddApplication();
builder.Services.AddBotHandlers();

var app = builder.Build();

app.Run();
