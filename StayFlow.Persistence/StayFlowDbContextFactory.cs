using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using StayFlow.Application.Common.Abstractions;

namespace StayFlow.Persistence;

/// <summary>
/// Design-time factory used by the EF Core tools (migrations). It does not need a live
/// database — only a provider configured to the right dialect — and resolves no tenant.
/// </summary>
public sealed class StayFlowDbContextFactory : IDesignTimeDbContextFactory<StayFlowDbContext>
{
    public StayFlowDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("STAYFLOW_DESIGNTIME_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=stayflow;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<StayFlowDbContext>()
            .UseNpgsql(connectionString, npgsql => npgsql.MigrationsAssembly(typeof(StayFlowDbContextFactory).Assembly.FullName))
            .Options;

        return new StayFlowDbContext(options, new DesignTimeTenantProvider());
    }

    private sealed class DesignTimeTenantProvider : ITenantProvider
    {
        public Guid? TenantId => null;
    }
}
