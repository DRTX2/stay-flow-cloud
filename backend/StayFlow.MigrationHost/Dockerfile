# syntax=docker/dockerfile:1
# Build context is the backend/ directory.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY .editorconfig StayFlowCloud.sln global.json Directory.Build.props Directory.Packages.props ./
COPY StayFlow.MigrationHost/StayFlow.MigrationHost.csproj StayFlow.MigrationHost/
COPY StayFlow.Application/StayFlow.Application.csproj StayFlow.Application/
COPY StayFlow.Domain/StayFlow.Domain.csproj StayFlow.Domain/
COPY StayFlow.Infrastructure/StayFlow.Infrastructure.csproj StayFlow.Infrastructure/
COPY StayFlow.Persistence/StayFlow.Persistence.csproj StayFlow.Persistence/
COPY StayFlow.Contracts/StayFlow.Contracts.csproj StayFlow.Contracts/
RUN dotnet restore StayFlow.MigrationHost/StayFlow.MigrationHost.csproj

COPY . .
RUN dotnet publish StayFlow.MigrationHost/StayFlow.MigrationHost.csproj -c $BUILD_CONFIGURATION -o /app --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
USER $APP_UID
COPY --from=build /app .
ENTRYPOINT ["dotnet", "StayFlow.MigrationHost.dll"]
