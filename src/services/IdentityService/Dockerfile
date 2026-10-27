FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/shared/CoreSolution.Shared/CoreSolution.Shared.csproj src/shared/CoreSolution.Shared/
COPY src/services/IdentityService/Identity.Core/Identity.Core.csproj src/services/IdentityService/Identity.Core/
COPY src/services/IdentityService/Identity.App/Identity.App.csproj src/services/IdentityService/Identity.App/
COPY src/services/IdentityService/Identity.Infrastructure/Identity.Infrastructure.csproj src/services/IdentityService/Identity.Infrastructure/
COPY src/services/IdentityService/Identity.Api/Identity.Api.csproj src/services/IdentityService/Identity.Api/

RUN dotnet restore src/services/IdentityService/Identity.Api/Identity.Api.csproj

COPY src/shared/CoreSolution.Shared/ src/shared/CoreSolution.Shared/
COPY src/services/IdentityService/ src/services/IdentityService/

RUN dotnet publish src/services/IdentityService/Identity.Api/Identity.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS runtime
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .

USER $APP_UID

ENTRYPOINT ["dotnet", "Identity.Api.dll"]
