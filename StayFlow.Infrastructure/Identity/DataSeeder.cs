using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
/// Applies migrations and seeds the baseline data the platform needs to be navigable on a fresh
/// database: OAuth2 clients and scopes, the built-in roles with their permission claims, a demo
/// tenant, a super-admin login and a little sample inventory. Idempotent — safe to run on boot.
/// </summary>
public sealed class DataSeeder(IServiceProvider serviceProvider, ILogger<DataSeeder> logger)
{
    // Demo credentials. Fine for a portfolio/dev environment; never ship real secrets like this.
    public const string AdminEmail = "admin@stayflow.local";
    public const string AdminPassword = "Admin123$";

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var services = scope.ServiceProvider;

        var context = services.GetRequiredService<StayFlowDbContext>();
        await context.Database.MigrateAsync(cancellationToken);

        await SeedScopesAsync(services, cancellationToken);
        await SeedClientsAsync(services, cancellationToken);
        await SeedRolesAsync(services);
        var tenantId = await SeedDemoTenantAsync(context, cancellationToken);
        await SeedAdminUserAsync(services, tenantId);
        await SeedSampleInventoryAsync(context, tenantId, cancellationToken);

        logger.LogInformation("Seed complete. Admin login: {Email} / {Password}", AdminEmail, AdminPassword);
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

    private static async Task SeedClientsAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        var manager = services.GetRequiredService<IOpenIddictApplicationManager>();

        if (await manager.FindByClientIdAsync(AuthConstants.Clients.Spa, cancellationToken) is null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = AuthConstants.Clients.Spa,
                ClientType = OpenIddictConstants.ClientTypes.Public,
                DisplayName = "StayFlow SPA (first-party)",
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.GrantTypes.Password,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                    OpenIddictConstants.Permissions.Prefixes.Scope + AuthConstants.ApiScope,
                    OpenIddictConstants.Permissions.Scopes.Profile,
                    OpenIddictConstants.Permissions.Scopes.Roles,
                },
            }, cancellationToken);
        }

        if (await manager.FindByClientIdAsync(AuthConstants.Clients.Service, cancellationToken) is null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = AuthConstants.Clients.Service,
                ClientSecret = AuthConstants.Clients.ServiceSecret,
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

    private static async Task<Guid> SeedDemoTenantAsync(StayFlowDbContext context, CancellationToken cancellationToken)
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
        return tenant.Id;
    }

    private async Task SeedAdminUserAsync(IServiceProvider services, Guid tenantId)
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        if (await userManager.FindByEmailAsync(AdminEmail) is not null)
        {
            return;
        }

        var admin = new ApplicationUser
        {
            UserName = AdminEmail,
            Email = AdminEmail,
            EmailConfirmed = true,
            FullName = "Platform Administrator",
            TenantId = tenantId,
        };

        var result = await userManager.CreateAsync(admin, AdminPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            logger.LogError("Failed to create admin user: {Errors}", errors);
            return;
        }

        await userManager.AddToRoleAsync(admin, Roles.SuperAdmin);
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
