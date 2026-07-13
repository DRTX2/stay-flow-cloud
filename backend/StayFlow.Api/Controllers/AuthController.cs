using System.Collections.Immutable;
using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using StayFlow.Application.Common.Authorization;
using StayFlow.Infrastructure;
using StayFlow.Persistence;
using StayFlow.Persistence.Identity;

namespace StayFlow.Api.Controllers;

/// <summary>
/// OAuth2 token endpoint and identity introspection. Handles the token-issuing grants:
/// - Authorization Code + PKCE exchange (primary interactive flow for human users)
/// - Client Credentials (machine-to-machine for ERP integrations)
/// - Refresh Token rotation (silent renewal)
///
/// Password (ROPC) grant is intentionally NOT supported. It cannot be made secure in a modern
/// SaaS: it exposes credentials to the client, blocks MFA, and breaks phishing-resistant auth.
/// The interactive authorize/login lives in <see cref="AuthorizeController"/> and
/// <see cref="AccountController"/>.
/// </summary>
[ApiController]
public sealed class AuthController(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    StayFlowDbContext dbContext,
    IWebHostEnvironment environment,
    IConfiguration configuration) : ControllerBase
{
    [HttpPost("~/connect/token"), Produces("application/json")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        if (request.IsClientCredentialsGrantType())
        {
            return await HandleClientCredentialsGrantAsync(request);
        }

        if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
        {
            return await HandleStoredPrincipalGrantAsync();
        }

        // Password grant is disabled. Return a clear error so clients that still attempt it
        // get an actionable message rather than a generic 400.
        if (request.IsPasswordGrantType())
        {
            return Forbidden(
                OpenIddictConstants.Errors.UnsupportedGrantType,
                "The password grant type is disabled. Use Authorization Code + PKCE.");
        }

        return Forbidden(OpenIddictConstants.Errors.UnsupportedGrantType, "The specified grant type is not supported.");
    }

    [Authorize]
    [HttpGet("~/api/v1/me"), Produces("application/json")]
    public IActionResult Me() => Ok(new
    {
        userId = User.FindFirstValue(OpenIddictConstants.Claims.Subject),
        name = User.FindFirstValue(OpenIddictConstants.Claims.Name) ?? User.Identity?.Name,
        tenantId = User.FindFirstValue(AuthConstants.TenantClaim),
        roles = User.FindAll(OpenIddictConstants.Claims.Role).Select(c => c.Value),
        permissions = User.FindAll(AuthConstants.PermissionClaim).Select(c => c.Value),
    });

    private async Task<Microsoft.AspNetCore.Mvc.SignInResult> HandleClientCredentialsGrantAsync(OpenIddictRequest request)
    {
        var identity = NewIdentity();
        identity.SetClaim(OpenIddictConstants.Claims.Subject, request.ClientId)
            .SetClaim(OpenIddictConstants.Claims.Name, request.ClientId);

        if (environment.IsDevelopment() && request.ClientId == AuthConstants.Clients.TestAdmin)
        {
            var tenantId = await dbContext.Tenants.IgnoreQueryFilters()
                .Where(tenant => tenant.Slug == "grand-demo")
                .Select(tenant => tenant.Id)
                .FirstOrDefaultAsync();

            if (tenantId != Guid.Empty)
            {
                identity.SetClaim(AuthConstants.TenantClaim, tenantId.ToString());
            }

            identity.SetClaims(AuthConstants.PermissionClaim, Permissions.All.ToImmutableArray());
            return SignInWith(identity, request.GetScopes());
        }

        if (!string.IsNullOrWhiteSpace(configuration["Authentication:SmokeClientId"])
            && request.ClientId == configuration["Authentication:SmokeClientId"])
        {
            var tenantSlug = configuration["Authentication:SmokeTenantSlug"] ?? "grand-demo";
            var tenantId = await dbContext.Tenants.IgnoreQueryFilters()
                .Where(tenant => tenant.Slug == tenantSlug)
                .Select(tenant => tenant.Id)
                .FirstOrDefaultAsync();

            if (tenantId != Guid.Empty)
            {
                identity.SetClaim(AuthConstants.TenantClaim, tenantId.ToString());
            }

            identity.SetClaims(AuthConstants.PermissionClaim, [Permissions.AnalyticsView]);
            return SignInWith(identity, request.GetScopes());
        }

        // Machine clients get read access to the public API surface.
        // Expand permissions here or inject them from client claims as needed.
        identity.SetClaims(AuthConstants.PermissionClaim,
        [
            Permissions.RoomsRead, Permissions.ReservationsRead, Permissions.GuestsRead,
        ]);

        return SignInWith(identity, request.GetScopes());
    }

    // Shared by the Authorization Code exchange and Refresh Token rotation: OpenIddict restores the
    // principal stashed when the code/refresh token was issued. Rebuild mutable account claims so
    // role changes, deactivation and an explicit guest link take effect on refresh.
    private async Task<IActionResult> HandleStoredPrincipalGrantAsync()
    {
        var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        var principal = result.Principal;
        if (principal is null)
        {
            return Forbidden(OpenIddictConstants.Errors.InvalidGrant, "The token is no longer valid.");
        }

        var userId = principal.GetClaim(OpenIddictConstants.Claims.Subject);
        if (userId is not null)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user is null || !user.IsActive)
            {
                return Forbidden(OpenIddictConstants.Errors.InvalidGrant, "The account is no longer active.");
            }

            var identity = (ClaimsIdentity)principal.Identity!;
            ReplaceClaim(identity, AuthConstants.TenantClaim, user.TenantId.ToString());
            ReplaceClaim(identity, AuthConstants.GuestClaim, user.GuestId?.ToString());
            foreach (var claim in identity.FindAll(AuthConstants.TenantClaim)
                         .Concat(identity.FindAll(AuthConstants.GuestClaim)))
            {
                claim.SetDestinations(OpenIddictConstants.Destinations.AccessToken);
            }

            foreach (var claim in identity.FindAll(OpenIddictConstants.Claims.Role).ToList())
            {
                identity.RemoveClaim(claim);
            }
            var roles = await userManager.GetRolesAsync(user);
            identity.SetClaims(OpenIddictConstants.Claims.Role, [.. roles]);
            foreach (var claim in identity.FindAll(OpenIddictConstants.Claims.Role))
            {
                claim.SetDestinations(principal.HasScope(OpenIddictConstants.Scopes.Roles)
                    ? [OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken]
                    : [OpenIddictConstants.Destinations.AccessToken]);
            }

            foreach (var claim in identity.FindAll(AuthConstants.PermissionClaim).ToList())
            {
                identity.RemoveClaim(claim);
            }
            identity.SetClaims(AuthConstants.PermissionClaim, [.. await ResolvePermissionsAsync(roles)]);
            foreach (var claim in identity.FindAll(AuthConstants.PermissionClaim))
            {
                claim.SetDestinations(OpenIddictConstants.Destinations.AccessToken);
            }
        }

        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private static void ReplaceClaim(ClaimsIdentity identity, string type, string? value)
    {
        foreach (var claim in identity.FindAll(type).ToList())
        {
            identity.RemoveClaim(claim);
        }
        if (value is not null)
        {
            identity.SetClaim(type, value);
        }
    }

    private async Task<IReadOnlyCollection<string>> ResolvePermissionsAsync(IEnumerable<string> roles)
    {
        var permissions = new HashSet<string>(StringComparer.Ordinal);
        foreach (var roleName in roles)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role is null)
            {
                continue;
            }

            foreach (var claim in await roleManager.GetClaimsAsync(role))
            {
                if (claim.Type == AuthConstants.PermissionClaim)
                {
                    permissions.Add(claim.Value);
                }
            }
        }

        return permissions;
    }

    private static ClaimsIdentity NewIdentity() => new(
        authenticationType: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
        nameType: OpenIddictConstants.Claims.Name,
        roleType: OpenIddictConstants.Claims.Role);

    private Microsoft.AspNetCore.Mvc.SignInResult SignInWith(ClaimsIdentity identity, ImmutableArray<string> scopes)
    {
        var principal = new ClaimsPrincipal(identity);
        principal.SetScopes(scopes);
        principal.SetResources("stayflow-api");

        foreach (var claim in principal.Claims)
        {
            claim.SetDestinations(OpenIddictConstants.Destinations.AccessToken);
        }

        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private ForbidResult Forbidden(string error, string description) => Forbid(
        authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
        properties: new AuthenticationProperties(new Dictionary<string, string?>
        {
            [OpenIddictServerAspNetCoreConstants.Properties.Error] = error,
            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = description,
        }));
}
