using System.Collections.Immutable;
using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using StayFlow.Application.Common.Authorization;
using StayFlow.Infrastructure;
using StayFlow.Persistence.Identity;

namespace StayFlow.Api.Controllers;

/// <summary>
/// OAuth2 token endpoint and identity introspection. Supports the password (first-party SPA) and
/// client-credentials (machine-to-machine) grants, plus refresh-token rotation. Authorization Code
/// + PKCE is the intended future addition once a login/consent UI exists.
/// </summary>
[ApiController]
public sealed class AuthController(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager) : ControllerBase
{
    [HttpPost("~/connect/token"), Produces("application/json")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        if (request.IsPasswordGrantType())
        {
            return await HandlePasswordGrantAsync(request);
        }

        if (request.IsClientCredentialsGrantType())
        {
            return HandleClientCredentialsGrant(request);
        }

        if (request.IsRefreshTokenGrantType())
        {
            return await HandleRefreshTokenGrantAsync();
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

    private async Task<IActionResult> HandlePasswordGrantAsync(OpenIddictRequest request)
    {
        var user = await userManager.FindByNameAsync(request.Username!);
        if (user is null || !user.IsActive || !await userManager.CheckPasswordAsync(user, request.Password!))
        {
            return Forbidden(OpenIddictConstants.Errors.InvalidGrant, "Invalid username or password.");
        }

        var identity = NewIdentity();
        identity.SetClaim(OpenIddictConstants.Claims.Subject, user.Id.ToString())
            .SetClaim(OpenIddictConstants.Claims.Name, user.UserName)
            .SetClaim(OpenIddictConstants.Claims.Email, user.Email)
            .SetClaim(AuthConstants.TenantClaim, user.TenantId.ToString());

        var roles = await userManager.GetRolesAsync(user);
        identity.SetClaims(OpenIddictConstants.Claims.Role, [.. roles]);
        identity.SetClaims(AuthConstants.PermissionClaim, [.. await ResolvePermissionsAsync(roles)]);

        return SignInWith(identity, request.GetScopes());
    }

    private Microsoft.AspNetCore.Mvc.SignInResult HandleClientCredentialsGrant(OpenIddictRequest request)
    {
        var identity = NewIdentity();
        identity.SetClaim(OpenIddictConstants.Claims.Subject, request.ClientId)
            .SetClaim(OpenIddictConstants.Claims.Name, request.ClientId);

        // Machine clients get read access to the public API surface for this demo.
        identity.SetClaims(AuthConstants.PermissionClaim,
        [
            Permissions.RoomsRead, Permissions.ReservationsRead, Permissions.GuestsRead,
        ]);

        return SignInWith(identity, request.GetScopes());
    }

    private async Task<IActionResult> HandleRefreshTokenGrantAsync()
    {
        var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        var principal = result.Principal;
        var userId = principal?.GetClaim(OpenIddictConstants.Claims.Subject);

        // Re-validate the user is still active before re-issuing tokens.
        if (userId is not null)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user is null || !user.IsActive)
            {
                return Forbidden(OpenIddictConstants.Errors.InvalidGrant, "The account is no longer active.");
            }
        }

        foreach (var claim in principal!.Claims)
        {
            claim.SetDestinations(OpenIddictConstants.Destinations.AccessToken);
        }

        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
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
