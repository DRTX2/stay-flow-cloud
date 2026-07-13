using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Application.Common.Authorization;
using StayFlow.Application.Common.Notifications;
using StayFlow.Infrastructure.Identity;
using StayFlow.Infrastructure.Notifications;
using StayFlow.Infrastructure.Observability;
using StayFlow.Infrastructure.Tenancy;
using StayFlow.Infrastructure.Time;
using StayFlow.Persistence;
using StayFlow.Persistence.Identity;

namespace StayFlow.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Configuration key for the frontend URL that hosts the interactive login form.
    /// The Identity cookie challenge redirects here so the backend never serves frontend HTML.
    /// </summary>
    public const string FrontendLoginUrlKey = "Authentication:FrontendLoginUrl";

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        bool isDevelopment = false,
        IConfiguration? configuration = null)
    {
        services.AddHttpContextAccessor();

        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<ITenantProvider, TenantProvider>();
        services.AddScoped<IFeatureService, FeatureService>();
        services.AddScoped<IStaffAdministrationService, StaffAdministrationService>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<StayFlowMetrics>();
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

        AddOpenIddict(services, configuration, isDevelopment);
        AddAuthenticationAndAuthorization(services, configuration, isDevelopment);

        return services;
    }

    private static void AddOpenIddict(IServiceCollection services, IConfiguration? configuration, bool isDevelopment)
    {
        var issuer = configuration?["Authentication:Issuer"] ?? "http://localhost:8080/";

        services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                    .UseDbContext<StayFlowDbContext>();
            })
            .AddServer(options =>
            {
                options.SetIssuer(new Uri(issuer));

                options.SetTokenEndpointUris("connect/token")
                    .SetAuthorizationEndpointUris("connect/authorize")
                    .SetEndSessionEndpointUris("connect/logout")
                    .SetUserInfoEndpointUris("connect/userinfo");

                // Authorization Code + PKCE is the primary (and only human-facing) interactive
                // grant. Password / ROPC is intentionally disabled — it cannot support MFA,
                // phishing-resistant auth, or proper token binding.
                // Client Credentials handles machine-to-machine (ERP, integrations).
                // Refresh Token enables silent renewal without re-authentication.
                options.AllowAuthorizationCodeFlow()
                    .RequireProofKeyForCodeExchange()
                    .AllowClientCredentialsFlow()
                    .AllowRefreshTokenFlow();

                options.RegisterScopes(
                    AuthConstants.ApiScope,
                    OpenIddictConstants.Scopes.OpenId,
                    OpenIddictConstants.Scopes.Email,
                    OpenIddictConstants.Scopes.Profile,
                    OpenIddictConstants.Scopes.Roles,
                    OpenIddictConstants.Scopes.OfflineAccess);

                if (isDevelopment)
                {
                    options.AddEphemeralEncryptionKey()
                        .AddEphemeralSigningKey();
                }
                else
                {
                    var certificate = LoadOpenIddictCertificate(configuration);
                    options.AddSigningCertificate(certificate)
                        .AddEncryptionCertificate(certificate);
                }

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

    internal static X509Certificate2 LoadOpenIddictCertificate(IConfiguration? configuration)
    {
        var encodedPfx = configuration?["Authentication:OpenIddict:CertificatePfxBase64"];
        var password = configuration?["Authentication:OpenIddict:CertificatePassword"];
        if (string.IsNullOrWhiteSpace(encodedPfx) || string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "Production requires Authentication:OpenIddict:CertificatePfxBase64 and CertificatePassword.");
        }

        X509Certificate2 certificate;
        try
        {
            certificate = X509CertificateLoader.LoadPkcs12(
                Convert.FromBase64String(encodedPfx), password,
                X509KeyStorageFlags.EphemeralKeySet);
        }
        catch (Exception exception) when (exception is FormatException or CryptographicException)
        {
            throw new InvalidOperationException("The configured OpenIddict PFX could not be loaded.", exception);
        }

        if (!certificate.HasPrivateKey)
        {
            throw new InvalidOperationException("The configured OpenIddict certificate has no private key.");
        }
        return certificate;
    }

    private static void AddAuthenticationAndAuthorization(
        IServiceCollection services,
        IConfiguration? configuration,
        bool isDevelopment)
    {
        // API calls authenticate with bearer tokens (OpenIddict validation) by default; the
        // interactive Authorization Code flow uses Identity's application cookie for the logged-in
        // user, and external providers sign in through the external cookie.
        var authentication = services.AddAuthentication(options =>
        {
            options.DefaultScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
        });

        // The interactive login form lives in the Next.js frontend (not the API).
        // The cookie challenge redirects there; the frontend page POSTs credentials
        // back to the API's /account/login endpoint.
        var frontendLoginUrl = configuration?[FrontendLoginUrlKey]
            ?? "http://localhost:3000/signin";

        authentication.AddCookie(IdentityConstants.ApplicationScheme, options =>
        {
            // LoginPath must be relative (PathString). We intercept the redirect below.
            options.LoginPath = "/account/login";
            options.LogoutPath = "/account/logout";
            options.Cookie.Name = "StayFlow.Identity";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
            options.Cookie.SecurePolicy = isDevelopment
                ? Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest
                : Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
            options.ExpireTimeSpan = TimeSpan.FromHours(1);
            options.Events.OnRedirectToLogin = context =>
            {
                // Override the default redirect to point to the Next.js frontend
                var returnUrl = context.Properties.RedirectUri ?? "/";
                var loginUrl = new Uri(frontendLoginUrl);
                var destination = new UriBuilder(loginUrl);
                var query = System.Web.HttpUtility.ParseQueryString(loginUrl.Query);
                query["ReturnUrl"] = returnUrl;
                destination.Query = query.ToString();
                context.Response.Redirect(destination.ToString());
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            };
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
    /// Authentication:Google|Microsoft|Facebook|GitHub), so the app runs without social credentials.
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

        var facebook = configuration.GetSection("Authentication:Facebook");
        if (facebook["AppId"] is { Length: > 0 } facebookId && facebook["AppSecret"] is { Length: > 0 } facebookSecret)
        {
            authentication.AddFacebook(options =>
            {
                options.AppId = facebookId;
                options.AppSecret = facebookSecret;
                options.SignInScheme = IdentityConstants.ExternalScheme;
                options.Fields.Add("email");
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
