using Atlas.TgBot;
using Atlas.TgBot.Extensions;
using Atlas.TgBot.Extensions.DependencyInjection;
using CoreSolution.Shared.TgBot.DependencyInjection;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddTelegramBot(builder.Configuration);
builder.Services.AddAtlasLogging(builder.Configuration);
builder.Services.AddBotInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddBotHandlers<AssemblyMarker>();

var app = builder.Build();

await app.ApplyMigrations();

app.Run();
