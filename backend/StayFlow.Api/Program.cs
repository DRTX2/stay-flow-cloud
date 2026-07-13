using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Formatting.Json;
using StayFlow.Api.Middleware;
using StayFlow.Api.Observability;
using StayFlow.Api.OpenApi;
using StayFlow.Application;
using StayFlow.Infrastructure;
using StayFlow.Infrastructure.Auditing;
using StayFlow.Infrastructure.Caching;
using StayFlow.Infrastructure.Identity;
using StayFlow.Infrastructure.Jobs;
using StayFlow.Infrastructure.Messaging;
using StayFlow.Infrastructure.Observability;
using StayFlow.Infrastructure.Storage;
using StayFlow.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("service", ObservabilityExtensions.ServiceName)
    .Enrich.With<ActivityLogEnricher>()
    .Enrich.With<SensitiveDataLogEnricher>()
    .WriteTo.Console(new JsonFormatter(renderMessage: true)));

var connectionString = builder.Configuration["STAYFLOW_APP_CONNECTION"]
    ?? builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");
connectionString = PostgreSqlConnectionString.Normalize(connectionString);

builder.Services.AddApplication();
builder.Services.AddPersistence(connectionString);
builder.Services.AddInfrastructure(builder.Environment.IsDevelopment(), builder.Configuration);
builder.Services.AddCaching(
    builder.Configuration.GetConnectionString("Redis"),
    builder.Configuration,
    builder.Environment.IsDevelopment());
builder.Services.AddAudit(builder.Configuration.GetConnectionString("Mongo"));
builder.Services.AddObservability(builder.Configuration);
builder.Services.AddMessaging(builder.Configuration);
builder.Services.AddBackgroundJobs(connectionString);
builder.Services.AddDocumentStorage(builder.Configuration);

// CORS: allow the Next.js frontend to POST credentials cross-origin to /account/login.
// The Identity application cookie is SameSite=None so it round-trips through the
// frontend → backend → frontend redirect chain correctly.
var frontendOrigin = builder.Configuration["FrontendOrigin"] ?? "http://localhost:3000";
builder.Services.AddCors(options =>
    options.AddPolicy("frontend", policy => policy
        .WithOrigins(frontendOrigin)
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials()));

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Default budget per tenant (falls back to client IP), keeps a noisy tenant from
    // starving others. A tighter named policy protects the token endpoint from brute force.
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.User.FindFirst("tenant_id")?.Value
                ?? context.Connection.RemoteIpAddress?.ToString()
                ?? "anonymous",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = 100, Window = TimeSpan.FromSeconds(10), QueueLimit = 0 }));

    options.AddFixedWindowLimiter("auth", limiter =>
    {
        limiter.PermitLimit = 5;
        limiter.Window = TimeSpan.FromSeconds(10);
        limiter.QueueLimit = 0;
    });
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Serialize enums as their string names (e.g. "Confirmed", "Available") so the public
        // JSON contract is human-readable and stable against numeric reordering. Accepts the name
        // (case-insensitive) or the integer on the way in too.
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "StayFlow Cloud API",
        Version = "v1",
        Description = "StayFlow Cloud Property Management System API"
    });

    // Include XML Comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    options.CustomOperationIds(description =>
        $"{description.ActionDescriptor.RouteValues["controller"]}_{description.ActionDescriptor.RouteValues["action"]}_{description.HttpMethod}"
            .ToLowerInvariant());
    options.TagActionsBy(description => [description.ActionDescriptor.RouteValues["controller"] ?? "API"]);
    options.OperationFilter<AuthorizationOperationFilter>();

    // Interactive clients use Authorization Code + PKCE; service integrations use client credentials.
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri("/connect/authorize", UriKind.Relative),
                TokenUrl = new Uri("/connect/token", UriKind.Relative),
                Scopes = new Dictionary<string, string>
                {
                    [AuthConstants.ApiScope] = "StayFlow API access",
                },
            },
            ClientCredentials = new OpenApiOAuthFlow
            {
                TokenUrl = new Uri("/connect/token", UriKind.Relative),
                Scopes = new Dictionary<string, string>
                {
                    [AuthConstants.ApiScope] = "StayFlow API access",
                },
            },
        },
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        { new OpenApiSecuritySchemeReference("oauth2", document, null), new List<string> { AuthConstants.ApiScope } },
    });
});

var app = builder.Build();

var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost,
    ForwardLimit = null
};
forwardedHeadersOptions.KnownIPNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeadersOptions);

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSerilogRequestLogging();

var docsEnabled = builder.Configuration.GetValue<bool?>("Docs:Enabled") ?? app.Environment.IsDevelopment();
if (docsEnabled)
{
    app.UseSwagger(options => options.RouteTemplate = "openapi/{documentName}.json");
    app.MapScalarApiReference("/docs", options =>
    {
        options.OpenApiRoutePattern = "/openapi/{documentName}.json";
        options.Authentication = new ScalarAuthenticationOptions
        {
            PreferredSecuritySchemes = ["oauth2"]
        };
    });
}

app.UseCors("frontend");

var metricsToken = builder.Configuration["Metrics:BearerToken"];
if (!string.IsNullOrWhiteSpace(metricsToken))
{
    app.UseWhen(
        context => context.Request.Path == "/metrics",
        branch => branch.Use(async (context, next) =>
        {
            var supplied = context.Request.Headers.Authorization.ToString();
            if (!string.Equals(supplied, $"Bearer {metricsToken}", StringComparison.Ordinal))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            await next(context);
        }));
}
app.UseAuthentication();
app.UseAuthorization();

// Rate limiting can be switched off (e.g. for integration tests that fetch tokens in a loop).
if (builder.Configuration.GetValue("RateLimiting:Enabled", true))
{
    app.UseRateLimiter();
}

app.UseSession();

app.MapControllers();
app.MapObservability();

var runMigrationsOnStartup = builder.Configuration.GetValue("Database:RunMigrationsOnStartup", false);
var ensureCreatedOnStartup = builder.Configuration.GetValue("Database:EnsureCreatedOnStartup", false);

if (runMigrationsOnStartup && ensureCreatedOnStartup)
{
    throw new InvalidOperationException(
        "Use either Database:RunMigrationsOnStartup or Database:EnsureCreatedOnStartup, not both.");
}

if (ensureCreatedOnStartup && !app.Environment.IsDevelopment())
{
    throw new InvalidOperationException(
        "Database:EnsureCreatedOnStartup is only allowed in Development because it bypasses EF migrations.");
}

if (runMigrationsOnStartup)
{
    await using var scope = app.Services.CreateAsyncScope();
    var context = scope.ServiceProvider.GetRequiredService<StayFlowDbContext>();
    await context.Database.MigrateAsync();
    await scope.ServiceProvider.GetRequiredService<DataSeeder>().SeedAsync();
}
else if (ensureCreatedOnStartup)
{
    await using var scope = app.Services.CreateAsyncScope();
    var context = scope.ServiceProvider.GetRequiredService<StayFlowDbContext>();
    await context.Database.EnsureCreatedAsync();
    await scope.ServiceProvider.GetRequiredService<DataSeeder>().SeedAsync();
}

app.UseBackgroundJobs();

// IMPORTANT: Migrations and seeding are NO LONGER run on API startup.
// Run them explicitly using the StayFlow.MigrationHost CLI tool before deploying the API:
//
//   dotnet run --project backend/StayFlow.MigrationHost -- migrate
//   dotnet run --project backend/StayFlow.MigrationHost -- seed
//
// This prevents race conditions in multi-replica deployments and separates the migrator
// database user (schema-level permissions) from the app user (data-level permissions only).
// The Database:RunMigrationsOnStartup switch exists only for integration/contract tests.
// For local throwaway databases, Database:EnsureCreatedOnStartup can create the schema
// directly from the current EF model without migrations. It does not update existing schemas.

app.Run();

/// <summary>Exposed so the integration-test host (WebApplicationFactory) can reference the entry point.</summary>
public partial class Program;
