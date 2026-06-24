# syntax=docker/dockerfile:1
# Build context is the backend/ directory (see compose.yaml), so COPY paths are backend-relative.

# --- Build stage -----------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy the solution + every project file (and the central package-management props)
# first so `dotnet restore` is cached as long as dependencies don't change.
COPY StayFlowCloud.sln global.json Directory.Build.props Directory.Packages.props ./
COPY StayFlow.Api/StayFlow.Api.csproj StayFlow.Api/
COPY StayFlow.Application/StayFlow.Application.csproj StayFlow.Application/
COPY StayFlow.Domain/StayFlow.Domain.csproj StayFlow.Domain/
COPY StayFlow.Infrastructure/StayFlow.Infrastructure.csproj StayFlow.Infrastructure/
COPY StayFlow.Persistence/StayFlow.Persistence.csproj StayFlow.Persistence/
COPY StayFlow.UnitTests/StayFlow.UnitTests.csproj StayFlow.UnitTests/
COPY StayFlow.IntegrationTests/StayFlow.IntegrationTests.csproj StayFlow.IntegrationTests/
RUN dotnet restore StayFlow.Api/StayFlow.Api.csproj

# Copy the rest of the source and publish the API.
COPY . .
RUN dotnet publish StayFlow.Api/StayFlow.Api.csproj -c $BUILD_CONFIGURATION -o /app --no-restore /p:UseAppHost=false

# --- Runtime stage ---------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Run as the non-root user provided by the base image.
USER $APP_UID

COPY --from=build /app .

# .NET base images default Kestrel to port 8080.
EXPOSE 8080
ENTRYPOINT ["dotnet", "StayFlow.Api.dll"]
