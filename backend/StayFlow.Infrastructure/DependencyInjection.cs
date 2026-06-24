using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Application.Common.Authorization;
using StayFlow.Application.Common.Notifications;
using StayFlow.Infrastructure.Identity;
using StayFlow.Infrastructure.Notifications;
using StayFlow.Infrastructure.Tenancy;
using StayFlow.Infrastructure.Time;
using StayFlow.Persistence;
using StayFlow.Persistence.Identity;

namespace StayFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        bool isDevelopment = false,
        IConfiguration? configuration = null)
    {
        services.AddHttpContextAccessor();

        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<ITenantProvider, TenantProvider>();
        services.AddScoped<IFeatureService, FeatureService>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<INotificationService, LoggingNotificationService>();
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
            .AddSignInManager()
            .AddDefaultTokenProviders();

        AddOpenIddict(services, isDevelopment);
        AddAuthenticationAndAuthorization(services, configuration);

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
                options.SetTokenEndpointUris("connect/token")
                    .SetAuthorizationEndpointUris("connect/authorize")
                    .SetEndSessionEndpointUris("connect/logout")
                    .SetUserInfoEndpointUris("connect/userinfo");

                options.AllowPasswordFlow()
                    .AllowClientCredentialsFlow()
                    .AllowRefreshTokenFlow();

                // Authorization Code + PKCE for the browser SPA and external API integrators.
                options.AllowAuthorizationCodeFlow()
                    .RequireProofKeyForCodeExchange();

                options.RegisterScopes(
                    AuthConstants.ApiScope,
                    OpenIddictConstants.Scopes.OpenId,
                    OpenIddictConstants.Scopes.Email,
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
                    .EnableTokenEndpointPassthrough()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableEndSessionEndpointPassthrough()
                    .EnableUserInfoEndpointPassthrough();

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

    private static void AddAuthenticationAndAuthorization(IServiceCollection services, IConfiguration? configuration)
    {
        // API calls authenticate with bearer tokens (OpenIddict validation) by default; the
        // interactive Authorization Code flow uses Identity's application cookie for the logged-in
        // user, and external providers sign in through the external cookie.
        var authentication = services.AddAuthentication(options =>
        {
            options.DefaultScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
        });

        authentication.AddCookie(IdentityConstants.ApplicationScheme, options =>
        {
            options.LoginPath = "/account/login";
            options.LogoutPath = "/account/logout";
            options.Cookie.Name = "StayFlow.Identity";
            options.Cookie.HttpOnly = true;
            options.ExpireTimeSpan = TimeSpan.FromHours(1);
        });

        authentication.AddCookie(IdentityConstants.ExternalScheme, options =>
        {
            options.Cookie.Name = "StayFlow.External";
            options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
        });

        // Registered so SignInManager (which signs these out) has handlers for every Identity scheme.
        authentication.AddCookie(IdentityConstants.TwoFactorRememberMeScheme);
        authentication.AddCookie(IdentityConstants.TwoFactorUserIdScheme);

        AddSocialLogin(authentication, configuration);

        var authorization = services.AddAuthorizationBuilder();

        // One policy per permission string: token must carry a matching "permission" claim.
        foreach (var permission in Permissions.All)
        {
            authorization.AddPolicy(permission, policy =>
                policy.RequireClaim(AuthConstants.PermissionClaim, permission));
        }
    }

    /// <summary>
    /// Registers external identity providers, each only when its credentials are configured (under
    /// Authentication:Google|Microsoft|GitHub), so the app builds and runs without any social setup.
    /// All providers sign in through the external cookie, which the account callback then links to a
    /// local user.
    /// </summary>
    private static void AddSocialLogin(AuthenticationBuilder authentication, IConfiguration? configuration)
    {
        if (configuration is null)
        {
            return;
        }

        var google = configuration.GetSection("Authentication:Google");
        if (google["ClientId"] is { Length: > 0 } googleId && google["ClientSecret"] is { Length: > 0 } googleSecret)
        {
            authentication.AddGoogle(options =>
            {
                options.ClientId = googleId;
                options.ClientSecret = googleSecret;
                options.SignInScheme = IdentityConstants.ExternalScheme;
            });
        }

        var microsoft = configuration.GetSection("Authentication:Microsoft");
        if (microsoft["ClientId"] is { Length: > 0 } microsoftId && microsoft["ClientSecret"] is { Length: > 0 } microsoftSecret)
        {
            authentication.AddMicrosoftAccount(options =>
            {
                options.ClientId = microsoftId;
                options.ClientSecret = microsoftSecret;
                options.SignInScheme = IdentityConstants.ExternalScheme;
            });
        }

        var github = configuration.GetSection("Authentication:GitHub");
        if (github["ClientId"] is { Length: > 0 } githubId && github["ClientSecret"] is { Length: > 0 } githubSecret)
        {
            authentication.AddGitHub(options =>
            {
                options.ClientId = githubId;
                options.ClientSecret = githubSecret;
                options.SignInScheme = IdentityConstants.ExternalScheme;
            });
        }
    }
}
