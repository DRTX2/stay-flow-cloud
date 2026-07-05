# syntax=docker/dockerfile:1
# Build context is the backend/ directory (see compose.yaml), so COPY paths are backend-relative.

# --- Build stage -----------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy the central package-management props and only the project files this service needs
# (itself + the shared contracts) so `dotnet restore` is cached until dependencies change.
COPY .editorconfig global.json Directory.Build.props Directory.Packages.props ./
COPY StayFlow.NotificationService/StayFlow.NotificationService.csproj StayFlow.NotificationService/
COPY StayFlow.Contracts/StayFlow.Contracts.csproj StayFlow.Contracts/
RUN dotnet restore StayFlow.NotificationService/StayFlow.NotificationService.csproj

# Copy the rest of the source and publish the service.
COPY StayFlow.NotificationService/ StayFlow.NotificationService/
COPY StayFlow.Contracts/ StayFlow.Contracts/
RUN dotnet publish StayFlow.NotificationService/StayFlow.NotificationService.csproj -c $BUILD_CONFIGURATION -o /app --no-restore /p:UseAppHost=false

# --- Runtime stage ---------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Run as the non-root user provided by the base image.
USER $APP_UID

COPY --from=build /app .

# .NET base images default Kestrel to port 8080.
EXPOSE 8080
ENTRYPOINT ["dotnet", "StayFlow.NotificationService.dll"]
