using Identity.Api.Extensions.Configuration;
using Identity.Api.Extensions.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServiceConfiguration(builder.Configuration, builder.Environment);

var app = builder.Build();

app.UseMiddlewareConfiguration();
await app.AddApplicationConfiguration();
app.MapRouteConfiguration();

app.Run();
