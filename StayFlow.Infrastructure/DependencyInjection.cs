using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Application.Common.Authorization;
using StayFlow.Infrastructure.Identity;
using StayFlow.Infrastructure.Tenancy;
using StayFlow.Infrastructure.Time;
using StayFlow.Persistence;
using StayFlow.Persistence.Identity;

namespace StayFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, bool isDevelopment = false)
    {
        services.AddHttpContextAccessor();

        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<ITenantProvider, TenantProvider>();
        services.AddScoped<IFeatureService, FeatureService>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<DataSeeder>();

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                // Align the principal's identifier/role claim types with what OpenIddict expects
                // when it materialises a principal from the access token.
                options.ClaimsIdentity.UserIdClaimType = OpenIddictConstants.Claims.Subject;
                options.ClaimsIdentity.UserNameClaimType = OpenIddictConstants.Claims.Name;
                options.ClaimsIdentity.RoleClaimType = OpenIddictConstants.Claims.Role;
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<StayFlowDbContext>()
            .AddDefaultTokenProviders();

        AddOpenIddict(services, isDevelopment);
        AddPermissionAuthorization(services);

        return services;
    }

    private static void AddOpenIddict(IServiceCollection services, bool isDevelopment)
    {
        services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                    .UseDbContext<StayFlowDbContext>();
            })
            .AddServer(options =>
            {
                options.SetTokenEndpointUris("connect/token");

                options.AllowPasswordFlow()
                    .AllowClientCredentialsFlow()
                    .AllowRefreshTokenFlow();

                options.RegisterScopes(
                    AuthConstants.ApiScope,
                    OpenIddictConstants.Scopes.OpenId,
                    OpenIddictConstants.Scopes.Profile,
                    OpenIddictConstants.Scopes.Roles,
                    OpenIddictConstants.Scopes.OfflineAccess);

                // Ephemeral keys keep dev frictionless on Linux (no cert store needed). Tokens
                // are invalidated on restart — acceptable for a demo. Swap for persisted X.509
                // certificates (or Secrets Manager-backed keys) in production.
                options.AddEphemeralEncryptionKey()
                    .AddEphemeralSigningKey();

                // Issue plain JWT access tokens so external API clients and Swagger can read them.
                options.DisableAccessTokenEncryption();

                var aspNetCore = options.UseAspNetCore()
                    .EnableTokenEndpointPassthrough();

                // Local development is served over plain HTTP; allow the token endpoint to accept it.
                // Production terminates TLS (at the edge or Kestrel) so this stays on by default there.
                if (isDevelopment)
                {
                    aspNetCore.DisableTransportSecurityRequirement();
                }
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
            });
    }

    private static void AddPermissionAuthorization(IServiceCollection services)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
        });

        var authorization = services.AddAuthorizationBuilder();

        // One policy per permission string: token must carry a matching "permission" claim.
        foreach (var permission in Permissions.All)
        {
            authorization.AddPolicy(permission, policy =>
                policy.RequireClaim(AuthConstants.PermissionClaim, permission));
        }
    }
}
