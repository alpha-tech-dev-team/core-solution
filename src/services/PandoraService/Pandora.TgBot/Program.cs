using CoreSolution.Shared.TgBot.DependencyInjection;
using Pandora.TgBot;
using Pandora.TgBot.Extension;
using Pandora.TgBot.Extension.DependencyInjection;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddTelegramBot(builder.Configuration);
builder.Services.AddPandoraOptions(builder.Configuration);
builder.Services.AddBotInfrastructure(builder.Configuration);
builder.Services.AddSecretStore(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddBotHandlers<AssemblyMarker>();

var app = builder.Build();

await app.ApplyMigrations();
await app.SeedDevelopmentData();

app.Run();
