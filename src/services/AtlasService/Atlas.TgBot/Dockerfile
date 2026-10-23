# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy everything
COPY . .

# Restore
RUN dotnet restore "src/services/AtlasService/Atlas.TgBot/Atlas.TgBot.csproj"

# Publish
WORKDIR "/app/src/services/AtlasService/Atlas.TgBot"
RUN dotnet publish "Atlas.TgBot.csproj" -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Non-root user
RUN useradd --no-create-home --shell /bin/bash appuser && chown -R appuser /app
USER appuser

COPY --from=build /app/publish .

# Env vars for production
ENV ASPNETCORE_ENVIRONMENT=Production
ENV Telegram__BotToken=""
ENV Access__AllowedChatIds=""

ENTRYPOINT ["dotnet", "Atlas.TgBot.dll"]