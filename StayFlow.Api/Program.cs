using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi;
using Serilog;
using StayFlow.Api.Middleware;
using StayFlow.Api.Observability;
using StayFlow.Application;
using StayFlow.Infrastructure;
using StayFlow.Infrastructure.Auditing;
using StayFlow.Infrastructure.Caching;
using StayFlow.Infrastructure.Identity;
using StayFlow.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .WriteTo.Console());

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

builder.Services.AddApplication();
builder.Services.AddPersistence(connectionString);
builder.Services.AddInfrastructure(builder.Environment.IsDevelopment());
builder.Services.AddCaching(builder.Configuration.GetConnectionString("Redis"));
builder.Services.AddAudit(builder.Configuration.GetConnectionString("Mongo"));
builder.Services.AddObservability(builder.Configuration);

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

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "StayFlow Cloud API", Version = "v1" });

    // Let Swagger UI obtain tokens from the password grant against the OpenIddict token endpoint.
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            Password = new OpenApiOAuthFlow
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

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.OAuthClientId(AuthConstants.Clients.Spa));
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

// Apply migrations and seed baseline data (clients, roles, admin, demo tenant) on startup.
await app.Services.GetRequiredService<DataSeeder>().SeedAsync();

app.Run();

/// <summary>Exposed so the integration-test host (WebApplicationFactory) can reference the entry point.</summary>
public partial class Program;
