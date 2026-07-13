using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace StayFlow.ContractTests;

/// <summary>
/// Boots the real API in-process against a throwaway PostgreSQL container so the public contract is
/// asserted against the actually-generated OpenAPI document and live responses.
/// </summary>
public sealed class ContractApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    public async Task InitializeAsync() => await _database.StartAsync();

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _database.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:Default", _database.GetConnectionString());
        builder.UseSetting("RateLimiting:Enabled", "false");
        builder.UseSetting("Metrics:BearerToken", "contract-metrics-token");
        // Contract tests exercise the current model, independently of migration authoring in progress.
        builder.UseSetting("Database:EnsureCreatedOnStartup", "true");
        builder.UseSetting("IsDevelopment", "true");
        builder.UseSetting("Seeding:AdminEmail", "admin@stayflow.local");
        builder.UseSetting("Seeding:AdminPassword", "Admin123$");
        builder.UseSetting("Authentication:ServiceClientSecret", "dev-service-secret-change-in-prod");
    }
}
