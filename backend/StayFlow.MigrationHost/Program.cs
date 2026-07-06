using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using StayFlow.Infrastructure;
using StayFlow.Infrastructure.Identity;
using StayFlow.Persistence;
using ILogger = Microsoft.Extensions.Logging.ILogger;

// ─────────────────────────────────────────────────────────────────────────────
// StayFlow.MigrationHost — database lifecycle CLI
//
// Usage:
//   dotnet run -- migrate              Apply pending EF Core migrations
//   dotnet run -- seed                 Seed baseline data (clients, roles, admin, demo tenant)
//   dotnet run -- rollback             Revert the last applied migration
//   dotnet run -- rollback --steps N   Revert the last N migrations
//   dotnet run -- rollback --target X  Revert down to (and including) migration X
//   dotnet run -- reset                Revert ALL migrations (empty schema)
//   dotnet run -- fresh                reset + migrate + seed  (dev only)
//   dotnet run -- status               List applied and pending migrations
//
// Connection strings:
//   STAYFLOW_MIGRATOR_CONNECTION  — used for migrate/rollback/reset (migrator user, DDL perms)
//   ConnectionStrings__Migrator    — environment fallback for DDL operations
//   STAYFLOW_APP_CONNECTION       — app user fallback
//   ConnectionStrings__Default    — app user fallback
//
// ─────────────────────────────────────────────────────────────────────────────

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var command = args.Length > 0 ? args[0].ToLowerInvariant() : "help";
    var remainingArgs = args.Skip(1).ToArray();

    var host = CreateHost(args);

    using var scope = host.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("MigrationHost");

    logger.LogInformation("StayFlow Migration Host — command: {Command}", command);

    switch (command)
    {
        case "migrate":
            await MigrateAsync(services, logger);
            break;

        case "seed":
            await SeedAsync(services, logger);
            break;

        case "rollback":
            await RollbackAsync(services, logger, remainingArgs);
            break;

        case "reset":
            await ResetAsync(services, logger);
            break;

        case "fresh":
            await FreshAsync(services, logger);
            break;

        case "status":
            await StatusAsync(services, logger);
            break;

        default:
            PrintHelp();
            break;
    }

    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Migration host failed");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}

// ─── Commands ────────────────────────────────────────────────────────────────

static async Task MigrateAsync(IServiceProvider services, ILogger logger)
{
    var context = GetMigratorContext(services);
    var pending = (await context.Database.GetPendingMigrationsAsync()).ToList();

    if (pending.Count == 0)
    {
        logger.LogInformation("No pending migrations. Database is up to date.");
        return;
    }

    logger.LogInformation("Applying {Count} pending migration(s): {Migrations}",
        pending.Count, string.Join(", ", pending));

    await context.Database.MigrateAsync();

    logger.LogInformation("Migrations applied successfully.");
}

static async Task SeedAsync(IServiceProvider services, ILogger logger)
{
    logger.LogInformation("Running data seeder...");
    var seeder = services.GetRequiredService<DataSeeder>();
    await seeder.SeedAsync();
    logger.LogInformation("Seed completed.");
}

static async Task RollbackAsync(IServiceProvider services, ILogger logger, string[] args)
{
    var context = GetMigratorContext(services);

    // Parse flags: --target <name> or --steps <N>
    string? targetMigration = null;
    int steps = 1;

    for (int i = 0; i < args.Length; i++)
    {
        if (args[i] == "--target" && i + 1 < args.Length)
        {
            targetMigration = args[i + 1];
            i++;
        }
        else if (args[i] == "--steps" && i + 1 < args.Length && int.TryParse(args[i + 1], out var n))
        {
            steps = n;
            i++;
        }
    }

    if (targetMigration is not null)
    {
        logger.LogInformation("Rolling back to migration: {Target}", targetMigration);
        await context.Database.MigrateAsync(targetMigration);
        logger.LogInformation("Rollback to '{Target}' completed.", targetMigration);
        return;
    }

    // Roll back N steps by finding the migration that is `steps` before the latest applied one.
    var applied = (await context.Database.GetAppliedMigrationsAsync()).ToList();
    if (applied.Count == 0)
    {
        logger.LogWarning("No migrations are applied. Nothing to roll back.");
        return;
    }

    if (steps >= applied.Count)
    {
        logger.LogInformation("Rolling back ALL {Count} migration(s) (steps requested: {Steps}).", applied.Count, steps);
        await ResetAsync(services, logger);
        return;
    }

    var rollbackTarget = applied[applied.Count - steps - 1];
    logger.LogInformation("Rolling back {Steps} step(s) to migration: {Target}", steps, rollbackTarget);
    await context.Database.MigrateAsync(rollbackTarget);
    logger.LogInformation("Rollback completed.");
}

static async Task ResetAsync(IServiceProvider services, ILogger logger)
{
    var context = GetMigratorContext(services);
    var applied = (await context.Database.GetAppliedMigrationsAsync()).ToList();

    if (applied.Count == 0)
    {
        logger.LogWarning("No migrations applied. Nothing to reset.");
        return;
    }

    logger.LogWarning("RESET: Reverting all {Count} migration(s). This will DROP all schema objects!", applied.Count);

    // "0" is EF Core's sentinel for "roll back everything".
    await context.Database.MigrateAsync("0");

    logger.LogInformation("Reset complete. Database schema is now empty.");
}

static async Task FreshAsync(IServiceProvider services, ILogger logger)
{
    var config = services.GetRequiredService<IConfiguration>();
    var isDev = config.GetValue<bool>("IsDevelopment");

    if (!isDev)
    {
        throw new InvalidOperationException(
            "'fresh' command is only allowed in development environments (IsDevelopment = true). " +
            "Use 'reset' + 'migrate' + 'seed' separately in production.");
    }

    logger.LogWarning("FRESH: This will reset the database and re-seed it from scratch!");
    await ResetAsync(services, logger);
    await MigrateAsync(services, logger);
    await SeedAsync(services, logger);
}

static async Task StatusAsync(IServiceProvider services, ILogger logger)
{
    var context = GetMigratorContext(services);
    var applied = (await context.Database.GetAppliedMigrationsAsync()).ToList();
    var pending = (await context.Database.GetPendingMigrationsAsync()).ToList();

    logger.LogInformation("Applied migrations ({Count}):", applied.Count);
    foreach (var m in applied)
    {
        logger.LogInformation("  ✓ {Migration}", m);
    }

    if (pending.Count > 0)
    {
        logger.LogInformation("Pending migrations ({Count}):", pending.Count);
        foreach (var m in pending)
        {
            logger.LogInformation("  ○ {Migration}", m);
        }
    }
    else
    {
        logger.LogInformation("No pending migrations. Database is up to date.");
    }
}

static void PrintHelp()
{
    Console.WriteLine("""
        StayFlow Migration Host

        Commands:
          migrate                   Apply all pending EF Core migrations
          seed                      Seed baseline data (roles, clients, admin user, demo tenant)
          rollback                  Revert the last applied migration
          rollback --steps N        Revert the last N applied migrations
          rollback --target NAME    Revert down to (but not including) the named migration
          reset                     Revert ALL migrations (drops schema)
          fresh                     reset + migrate + seed (development only)
          status                    Show applied and pending migrations

        Connection strings (checked in order):
          STAYFLOW_MIGRATOR_CONNECTION  For DDL operations (CREATE/ALTER/DROP TABLE)
          ConnectionStrings__Migrator    Environment fallback for DDL operations
          STAYFLOW_APP_CONNECTION       App user fallback
          ConnectionStrings__Default    App user fallback

        Examples:
          dotnet run -- migrate
          dotnet run -- rollback --steps 2
          dotnet run -- rollback --target 20260622202801_InitialCreate
          dotnet run -- fresh
        """);
}

// ─── Helpers ─────────────────────────────────────────────────────────────────

static StayFlowDbContext GetMigratorContext(IServiceProvider services)
{
    return services.GetRequiredService<StayFlowDbContext>();
}

static IHost CreateHost(string[] args)
{
    return Host.CreateDefaultBuilder(args)
        .UseSerilog((ctx, config) => config
            .ReadFrom.Configuration(ctx.Configuration)
            .WriteTo.Console())
        .ConfigureAppConfiguration((ctx, builder) =>
        {
            builder
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{ctx.HostingEnvironment.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            // Propagate IsDevelopment as a config value so DataSeeder can read it.
            builder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["IsDevelopment"] = ctx.HostingEnvironment.IsDevelopment().ToString().ToLower(),
            });
        })
        .ConfigureServices((ctx, services) =>
        {
            // The migrator uses the dedicated migrator connection string (DDL-level perms).
            // Fall back to the app connection string if the migrator one is not set.
            var migratorConnection = ResolveConnectionString(ctx)
                ?? throw new InvalidOperationException(
                    "No database connection string found. Set STAYFLOW_MIGRATOR_CONNECTION " +
                    "or ConnectionStrings__Default.");
            migratorConnection = PostgreSqlConnectionString.Normalize(migratorConnection);

            services.AddDbContext<StayFlowDbContext>((sp, options) =>
            {
                options.UseNpgsql(migratorConnection, npgsql =>
                {
                    npgsql.MigrationsAssembly(typeof(StayFlowDbContext).Assembly.FullName);
                    npgsql.EnableRetryOnFailure();
                });
                options.UseOpenIddict();
            });

            // Register Infrastructure services needed by DataSeeder (Identity, OpenIddict).
            services.AddInfrastructure(
                isDevelopment: ctx.HostingEnvironment.IsDevelopment(),
                configuration: ctx.Configuration);
        })
        .Build();
}

static string? ResolveConnectionString(HostBuilderContext ctx)
{
    var connectionString =
        Environment.GetEnvironmentVariable("STAYFLOW_MIGRATOR_CONNECTION")
        ?? Environment.GetEnvironmentVariable("ConnectionStrings__Migrator")
        ?? Environment.GetEnvironmentVariable("STAYFLOW_APP_CONNECTION")
        ?? Environment.GetEnvironmentVariable("ConnectionStrings__Default");

    if (!string.IsNullOrWhiteSpace(connectionString))
    {
        return connectionString;
    }

    if (!ctx.HostingEnvironment.IsDevelopment())
    {
        return null;
    }

    return ctx.Configuration.GetConnectionString("Migrator")
        ?? ctx.Configuration.GetConnectionString("Default");
}
