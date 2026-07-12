using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;
using StayFlow.Application.Common.Authorization;
using StayFlow.Domain.Guests;
using StayFlow.Domain.Rooms;
using StayFlow.Domain.Tenants;
using StayFlow.Persistence;
using StayFlow.Persistence.Identity;

namespace StayFlow.Infrastructure.Identity;

/// <summary>
/// Seeds the baseline data the platform needs to be navigable on a fresh database: OAuth2 clients
/// and scopes, the built-in roles with their permission claims, a demo tenant, a super-admin login
/// and sample inventory.
///
/// IMPORTANT: This seeder must ONLY be invoked from the StayFlow.MigrationHost CLI tool or from
/// integration-test setup. It must never run automatically on API startup in production.
///
/// Idempotent — safe to run multiple times.
/// </summary>
public sealed class DataSeeder(
    IServiceProvider serviceProvider,
    ILogger<DataSeeder> logger,
    IConfiguration configuration)
{
    // Configuration keys — values come from environment variables or secrets, never from source code.
    internal const string AdminEmailKey = "Seeding:AdminEmail";
    internal const string AdminPasswordKey = "Seeding:AdminPassword";
    internal const string ServiceClientSecretKey = "Authentication:ServiceClientSecret";
    private const string SmokeClientIdKey = "Authentication:SmokeClientId";
    private const string SmokeClientSecretKey = "Authentication:SmokeClientSecret";

    // Fallback defaults are ONLY used when running in Development and the keys are absent.
    // They are intentionally weak/obvious so developers know immediately this is not for production.
    private const string DevFallbackEmail = "admin@stayflow.local";
    private const string DevFallbackPassword = "Admin123$";
    private const string DevFallbackServiceSecret = "dev-service-secret-change-in-prod";
    private const string DevFallbackTestAdminSecret = "dev-test-admin-secret";

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var services = scope.ServiceProvider;

        await SeedScopesAsync(services, cancellationToken);
        await SeedClientsAsync(services, configuration, cancellationToken);
        await SeedRolesAsync(services);

        var context = services.GetRequiredService<StayFlowDbContext>();
        var tenantId = await SeedDemoTenantAsync(context, cancellationToken);
        await SeedAdminUserAsync(services, configuration, tenantId);
        await SeedSampleInventoryAsync(context, tenantId, cancellationToken);

        logger.LogInformation("Seed complete. Check Seeding:AdminEmail configuration for login credentials.");
    }

    private static async Task SeedScopesAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        var manager = services.GetRequiredService<IOpenIddictScopeManager>();
        if (await manager.FindByNameAsync(AuthConstants.ApiScope, cancellationToken) is not null)
        {
            return;
        }

        await manager.CreateAsync(new OpenIddictScopeDescriptor
        {
            Name = AuthConstants.ApiScope,
            DisplayName = "StayFlow API access",
            Resources = { "stayflow-api" },
        }, cancellationToken);
    }

    private static async Task SeedClientsAsync(
        IServiceProvider services,
        IConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var manager = services.GetRequiredService<IOpenIddictApplicationManager>();
        var isDev = configuration.GetValue<bool>("IsDevelopment");

        var spaRedirectUris = GetConfiguredUris(
            configuration,
            "Authentication:SpaRedirectUris",
            ["http://localhost:3000/api/auth/callback", "http://localhost:5173/callback"]);
        var spaPostLogoutRedirectUris = GetConfiguredUris(
            configuration,
            "Authentication:SpaPostLogoutRedirectUris",
            ["http://localhost:3000/", "http://localhost:5173/"]);

        var spaApplication = await manager.FindByClientIdAsync(AuthConstants.Clients.Spa, cancellationToken);
        if (spaApplication is null)
        {
            var spaClient = new OpenIddictApplicationDescriptor
            {
                ClientId = AuthConstants.Clients.Spa,
                ClientType = OpenIddictConstants.ClientTypes.Public,
                DisplayName = "StayFlow SPA (first-party)",
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.Endpoints.Authorization,
                    OpenIddictConstants.Permissions.Endpoints.EndSession,
                    // Authorization Code + PKCE is the ONLY interactive grant.
                    // Password / ROPC grant is disabled — it is a legacy flow that cannot
                    // support MFA, phishing-resistant auth, or proper token storage.
                    OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                    OpenIddictConstants.Permissions.ResponseTypes.Code,
                    OpenIddictConstants.Permissions.Prefixes.Scope + AuthConstants.ApiScope,
                    OpenIddictConstants.Permissions.Scopes.Email,
                    OpenIddictConstants.Permissions.Scopes.Profile,
                    OpenIddictConstants.Permissions.Scopes.Roles,
                },
                Requirements =
                {
                    OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange,
                },
            };

            foreach (var redirectUri in spaRedirectUris)
            {
                spaClient.RedirectUris.Add(redirectUri);
            }

            foreach (var postLogoutRedirectUri in spaPostLogoutRedirectUris)
            {
                spaClient.PostLogoutRedirectUris.Add(postLogoutRedirectUri);
            }

            await manager.CreateAsync(spaClient, cancellationToken);
        }
        else
        {
            var descriptor = new OpenIddictApplicationDescriptor();
            await manager.PopulateAsync(descriptor, spaApplication, cancellationToken);

            var changed = false;
            foreach (var redirectUri in spaRedirectUris.Where(uri => !descriptor.RedirectUris.Contains(uri)))
            {
                descriptor.RedirectUris.Add(redirectUri);
                changed = true;
            }

            foreach (var postLogoutRedirectUri in spaPostLogoutRedirectUris.Where(uri => !descriptor.PostLogoutRedirectUris.Contains(uri)))
            {
                descriptor.PostLogoutRedirectUris.Add(postLogoutRedirectUri);
                changed = true;
            }

            if (changed)
            {
                await manager.UpdateAsync(spaApplication, descriptor, cancellationToken);
            }
        }

        var serviceApplication = await manager.FindByClientIdAsync(AuthConstants.Clients.Service, cancellationToken);
        if (serviceApplication is null)
        {
            // Secret must come from configuration (environment variable / secrets manager).
            // In development without the key set, a fallback is used and a warning is emitted.
            var serviceSecret = configuration[ServiceClientSecretKey];
            if (string.IsNullOrWhiteSpace(serviceSecret))
            {
                if (!isDev)
                {
                    throw new InvalidOperationException(
                        $"Configuration key '{ServiceClientSecretKey}' is required in non-development environments. " +
                        "Set it via an environment variable or secrets manager.");
                }

                serviceSecret = DevFallbackServiceSecret;
            }

            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = AuthConstants.Clients.Service,
                ClientSecret = serviceSecret,
                ClientType = OpenIddictConstants.ClientTypes.Confidential,
                DisplayName = "StayFlow service (machine-to-machine)",
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                    OpenIddictConstants.Permissions.Prefixes.Scope + AuthConstants.ApiScope,
                },
            }, cancellationToken);
        }
        else
        {
            var serviceSecret = configuration[ServiceClientSecretKey];
            if (string.IsNullOrWhiteSpace(serviceSecret))
            {
                if (!isDev)
                {
                    throw new InvalidOperationException(
                        $"Configuration key '{ServiceClientSecretKey}' is required in non-development environments.");
                }

                serviceSecret = DevFallbackServiceSecret;
            }

            var descriptor = new OpenIddictApplicationDescriptor();
            await manager.PopulateAsync(descriptor, serviceApplication, cancellationToken);
            descriptor.ClientSecret = serviceSecret;
            await manager.UpdateAsync(serviceApplication, descriptor, cancellationToken);
        }

        if (isDev && await manager.FindByClientIdAsync(AuthConstants.Clients.TestAdmin, cancellationToken) is null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = AuthConstants.Clients.TestAdmin,
                ClientSecret = DevFallbackTestAdminSecret,
                ClientType = OpenIddictConstants.ClientTypes.Confidential,
                DisplayName = "StayFlow test admin (development only)",
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                    OpenIddictConstants.Permissions.Prefixes.Scope + AuthConstants.ApiScope,
                },
            }, cancellationToken);
        }

        var smokeClientId = configuration[SmokeClientIdKey];
        var smokeClientSecret = configuration[SmokeClientSecretKey];
        if (!string.IsNullOrWhiteSpace(smokeClientId) && !string.IsNullOrWhiteSpace(smokeClientSecret)
            && await manager.FindByClientIdAsync(smokeClientId, cancellationToken) is null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = smokeClientId,
                ClientSecret = smokeClientSecret,
                ClientType = OpenIddictConstants.ClientTypes.Confidential,
                DisplayName = "StayFlow staging smoke tests",
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                    OpenIddictConstants.Permissions.Prefixes.Scope + AuthConstants.ApiScope,
                },
            }, cancellationToken);
        }
    }

    private static Uri[] GetConfiguredUris(
        IConfiguration configuration,
        string key,
        IReadOnlyCollection<string> defaultValues)
    {
        var values = configuration.GetSection(key)
            .GetChildren()
            .Select(section => section.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!)
            .ToArray();

        if (values.Length == 0)
        {
            values = defaultValues.ToArray();
        }

        return values.Select(value => new Uri(value, UriKind.Absolute)).ToArray();
    }

    private static async Task SeedRolesAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

        foreach (var (roleName, permissions) in Roles.PermissionsByRole)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role is null)
            {
                role = new ApplicationRole(roleName) { Description = $"Built-in {roleName} role." };
                await roleManager.CreateAsync(role);
            }

            var existing = (await roleManager.GetClaimsAsync(role))
                .Where(c => c.Type == AuthConstants.PermissionClaim)
                .Select(c => c.Value)
                .ToHashSet(StringComparer.Ordinal);

            foreach (var permission in permissions.Where(p => !existing.Contains(p)))
            {
                await roleManager.AddClaimAsync(role, new Claim(AuthConstants.PermissionClaim, permission));
            }
        }
    }

    private async Task<Guid> SeedDemoTenantAsync(StayFlowDbContext context, CancellationToken cancellationToken)
    {
        const string slug = "grand-demo";
        var tenant = await context.Tenants.IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Slug == slug, cancellationToken);
        if (tenant is not null)
        {
            return tenant.Id;
        }

        tenant = Tenant.Create("Grand Demo Hotel", slug, PropertyType.Hotel, "USD");
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Demo tenant created: {Slug}", slug);
        return tenant.Id;
    }

    private async Task SeedAdminUserAsync(IServiceProvider services, IConfiguration config, Guid tenantId)
    {
        var isDev = config.GetValue<bool>("IsDevelopment");

        var adminEmail = config[AdminEmailKey];
        var adminPassword = config[AdminPasswordKey];

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            if (!isDev)
            {
                throw new InvalidOperationException(
                    $"Configuration keys '{AdminEmailKey}' and '{AdminPasswordKey}' are required in non-development environments. " +
                    "Set them via environment variables or a secrets manager.");
            }

            adminEmail ??= DevFallbackEmail;
            adminPassword ??= DevFallbackPassword;

            logger.LogWarning(
                "Admin credentials not configured. Using development fallbacks. " +
                "Set '{EmailKey}' and '{PasswordKey}' in configuration before deploying to production.",
                AdminEmailKey, AdminPasswordKey);
        }

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = "Platform Administrator",
                TenantId = tenantId,
            };

            var result = await userManager.CreateAsync(admin, adminPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                logger.LogError("Failed to create admin user: {Errors}", errors);
                return;
            }

            logger.LogInformation("Admin user created: {Email}", adminEmail);
        }
        else if (!await userManager.CheckPasswordAsync(admin, adminPassword))
        {
            if (await userManager.HasPasswordAsync(admin))
            {
                var removePassword = await userManager.RemovePasswordAsync(admin);
                if (!removePassword.Succeeded)
                {
                    var errors = string.Join("; ", removePassword.Errors.Select(e => e.Description));
                    logger.LogError("Failed to remove existing admin password: {Errors}", errors);
                    return;
                }
            }

            var addPassword = await userManager.AddPasswordAsync(admin, adminPassword);
            if (!addPassword.Succeeded)
            {
                var errors = string.Join("; ", addPassword.Errors.Select(e => e.Description));
                logger.LogError("Failed to update admin password: {Errors}", errors);
                return;
            }

            logger.LogInformation("Admin user password synchronized: {Email}", adminEmail);
        }

        if (!await userManager.IsInRoleAsync(admin, Roles.SuperAdmin))
        {
            await userManager.AddToRoleAsync(admin, Roles.SuperAdmin);
        }

        // Log only the email, NEVER the password.
    }

    private static async Task SeedSampleInventoryAsync(StayFlowDbContext context, Guid tenantId, CancellationToken cancellationToken)
    {
        var hasRooms = await context.Rooms.IgnoreQueryFilters().AnyAsync(r => r.TenantId == tenantId, cancellationToken);
        if (hasRooms)
        {
            return;
        }

        var standard = RoomType.Create("Standard Double", 90m, 2, "A comfortable double room.");
        var suite = RoomType.Create("Executive Suite", 220m, 4, "Spacious suite with lounge.");
        standard.TenantId = tenantId;
        suite.TenantId = tenantId;
        context.RoomTypes.AddRange(standard, suite);

        var rooms = new[]
        {
            Room.Create("101", standard.Id, 90m, 2, 1),
            Room.Create("102", standard.Id, 90m, 2, 1),
            Room.Create("201", suite.Id, 220m, 4, 2),
        };
        foreach (var room in rooms)
        {
            room.TenantId = tenantId;
        }
        context.Rooms.AddRange(rooms);

        var guest = Guest.Create("Ada", "Lovelace", "ada@example.com", "+1-202-555-0142");
        guest.TenantId = tenantId;
        context.Guests.Add(guest);

        await context.SaveChangesAsync(cancellationToken);
    }
}
