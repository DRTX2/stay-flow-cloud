using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace StayFlow.Infrastructure.Jobs;

public static class BackgroundJobsExtensions
{
    /// <summary>
    /// Registers Hangfire with PostgreSQL storage (its schema is created on first run) plus the job
    /// classes. The processing server is hosted in-process for the modular monolith.
    /// </summary>
    public static IServiceCollection AddBackgroundJobs(this IServiceCollection services, string connectionString)
    {
        services.AddHangfire(configuration => configuration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString)));

        services.AddHangfireServer();

        // Jobs are resolved per execution from the DI scope created by Hangfire.AspNetCore.
        services.AddScoped<NightAuditJob>();
        services.AddScoped<OccupancyCalculationJob>();
        services.AddScoped<ReminderEmailsJob>();
        services.AddScoped<InvoiceGenerationJob>();
        services.AddScoped<OutboxCleanupJob>();

        return services;
    }

    /// <summary>
    /// Maps the dashboard and registers the recurring schedule. All times are UTC (Hangfire's
    /// default), matching how the rest of the system records time.
    /// </summary>
    public static WebApplication UseBackgroundJobs(this WebApplication app)
    {
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = [new HangfireDashboardAuthorizationFilter(app.Environment.IsDevelopment())],
        });

        var recurringJobs = app.Services.GetRequiredService<IRecurringJobManager>();

        recurringJobs.AddOrUpdate<InvoiceGenerationJob>(
            "invoice-generation", job => job.ExecuteAsync(CancellationToken.None), Cron.Daily(2));

        recurringJobs.AddOrUpdate<NightAuditJob>(
            "night-audit", job => job.ExecuteAsync(CancellationToken.None), Cron.Daily(3));

        recurringJobs.AddOrUpdate<OccupancyCalculationJob>(
            "occupancy-calculation", job => job.ExecuteAsync(CancellationToken.None), Cron.Daily(4));

        recurringJobs.AddOrUpdate<ReminderEmailsJob>(
            "reminder-emails", job => job.ExecuteAsync(CancellationToken.None), Cron.Daily(8));

        recurringJobs.AddOrUpdate<OutboxCleanupJob>(
            "outbox-cleanup", job => job.ExecuteAsync(CancellationToken.None), Cron.Hourly());

        return app;
    }
}
