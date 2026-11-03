FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore "src/services/AtlasService/Atlas.TgBot/Atlas.TgBot.csproj"

RUN dotnet publish "src/services/AtlasService/Atlas.TgBot/Atlas.TgBot.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV DOTNET_ENVIRONMENT=Production

USER $APP_UID

ENTRYPOINT ["dotnet", "Atlas.TgBot.dll"]
