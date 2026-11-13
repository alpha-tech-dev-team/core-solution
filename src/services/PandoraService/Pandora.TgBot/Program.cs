using CoreSolution.Shared.Infrastructure.Configuration;
using CoreSolution.Shared.TgBot.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pandora.Infrastructure.Database.Seeding;
using Pandora.TgBot;
using Pandora.TgBot.Extension.DependencyInjection;

var builder = Host.CreateApplicationBuilder(args);

builder.AddSharedConfiguration();

builder.Services.AddTelegramBot(builder.Configuration);
builder.Services.AddPandoraOptions(builder.Configuration);
builder.Services.AddBotInfrastructure(builder.Configuration);
builder.Services.AddSecretStore(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddBotHandlers<AssemblyMarker>();

var app = builder.Build();

await DevelopmentDataSeeder.SeedIfEnabledAsync(app.Services, app.Services.GetRequiredService<IHostEnvironment>().IsDevelopment());

app.Run();
