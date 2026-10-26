FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src ./src
RUN find /src -type d \( -name bin -o -name obj \) -prune -exec rm -rf {} + \
 && dotnet publish src/services/PandoraService/Pandora.TgBot/Pandora.TgBot.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Pandora.TgBot.dll"]
