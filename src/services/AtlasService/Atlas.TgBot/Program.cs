using Atlas.TgBot;
using Atlas.TgBot.Extensions;
using Atlas.TgBot.Extensions.DependencyInjection;
using CoreSolution.Shared.TgBot.DependencyInjection;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddAtlasLogging(builder.Configuration);
builder.Services.AddBotInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration, builder.Environment);
builder.Services.AddBotHandlers<AssemblyMarker>();
builder.Services.AddTelegramBot(builder.Configuration);

var app = builder.Build();

await app.ApplyMigrations();

app.Run();
