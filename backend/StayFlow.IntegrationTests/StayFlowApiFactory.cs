using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace StayFlow.IntegrationTests;

/// <summary>
/// Boots the real API in-process against a throwaway PostgreSQL container.
/// The test factory enables Database:RunMigrationsOnStartup so the throwaway database is prepared
/// during test host startup. Production does not set that switch.
/// </summary>
public sealed class StayFlowApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    public string DatabaseConnectionString => _database.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _database.StartAsync();
    }

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
        builder.UseSetting("Database:RunMigrationsOnStartup", "true");
        // Dev fallback credentials for seeding in tests.
        builder.UseSetting("IsDevelopment", "true");
        builder.UseSetting("Seeding:AdminEmail", "admin@stayflow.local");
        builder.UseSetting("Seeding:AdminPassword", "Admin123$");
        builder.UseSetting("Authentication:ServiceClientSecret", "dev-service-secret-change-in-prod");
    }
}
