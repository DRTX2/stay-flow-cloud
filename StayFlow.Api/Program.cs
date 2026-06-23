using Microsoft.OpenApi;
using Serilog;
using StayFlow.Api.Middleware;
using StayFlow.Application;
using StayFlow.Infrastructure;
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

app.MapControllers();

// Apply migrations and seed baseline data (clients, roles, admin, demo tenant) on startup.
await app.Services.GetRequiredService<DataSeeder>().SeedAsync();

app.Run();

/// <summary>Exposed so the integration-test host (WebApplicationFactory) can reference the entry point.</summary>
public partial class Program;
