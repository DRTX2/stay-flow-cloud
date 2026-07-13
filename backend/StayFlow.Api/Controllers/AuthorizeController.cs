using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using StayFlow.Application.Common.Authorization;
using StayFlow.Infrastructure;
using StayFlow.Persistence.Identity;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace StayFlow.Api.Controllers;

/// <summary>
/// Interactive OpenID Connect endpoints for the Authorization Code + PKCE flow: authorize (issues
/// the code for a logged-in user), userinfo, and logout. Login itself is handled by
/// <see cref="AccountController"/> via the Identity application cookie.
/// </summary>
public sealed class AuthorizeController(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager) : ControllerBase
{
    [HttpGet("~/connect/authorize")]
    [HttpPost("~/connect/authorize")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Authorize()
    {
        var request = HttpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        var result = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);

        // Not signed in (or the client forced re-authentication): bounce to the login page and come
        // back to this exact authorize request afterwards.
        if (!result.Succeeded || request.HasPromptValue(PromptValues.Login))
        {
            return Challenge(
                authenticationSchemes: IdentityConstants.ApplicationScheme,
                properties: new AuthenticationProperties
                {
                    RedirectUri = Request.PathBase + Request.Path + QueryString.Create(Request.HasFormContentType
                        ? Request.Form.ToList()
                        : Request.Query.ToList()),
                });
        }

        var user = await userManager.GetUserAsync(result.Principal)
            ?? throw new InvalidOperationException("The logged-in user could not be resolved.");

        var identity = new ClaimsIdentity(
            authenticationType: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            nameType: Claims.Name,
            roleType: Claims.Role);

        identity.SetClaim(Claims.Subject, user.Id.ToString())
            .SetClaim(Claims.Name, user.UserName)
            .SetClaim(Claims.Email, user.Email)
            .SetClaim(AuthConstants.TenantClaim, user.TenantId.ToString());
        if (user.GuestId is { } guestId)
        {
            identity.SetClaim(AuthConstants.GuestClaim, guestId.ToString());
        }

        var roles = await userManager.GetRolesAsync(user);
        identity.SetClaims(Claims.Role, [.. roles]);
        identity.SetClaims(AuthConstants.PermissionClaim, [.. await ResolvePermissionsAsync(roles)]);

        var principal = new ClaimsPrincipal(identity);
        principal.SetScopes(request.GetScopes());
        principal.SetResources("stayflow-api");

        foreach (var claim in principal.Claims)
        {
            claim.SetDestinations(GetDestinations(claim, principal));
        }

        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [Authorize]
    [HttpGet("~/connect/userinfo")]
    [HttpPost("~/connect/userinfo")]
    [Produces("application/json")]
    public async Task<IActionResult> UserInfo()
    {
        var user = await userManager.FindByIdAsync(User.GetClaim(Claims.Subject) ?? string.Empty);
        if (user is null)
        {
            return Challenge(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidToken,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user no longer exists.",
                }));
        }

        var claims = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            [Claims.Subject] = user.Id.ToString(),
            [AuthConstants.TenantClaim] = user.TenantId.ToString(),
        };
        if (user.GuestId is { } guestId)
        {
            claims[AuthConstants.GuestClaim] = guestId.ToString();
        }

        if (User.HasScope(Scopes.Profile))
        {
            claims[Claims.Name] = user.UserName ?? string.Empty;
            claims[Claims.Role] = await userManager.GetRolesAsync(user);
        }

        if (User.HasScope(Scopes.Email) && user.Email is not null)
        {
            claims[Claims.Email] = user.Email;
            claims[Claims.EmailVerified] = user.EmailConfirmed;
        }

        return Ok(claims);
    }

    [HttpGet("~/connect/logout")]
    [HttpPost("~/connect/logout")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Logout()
    {
        var request = HttpContext.GetOpenIddictServerRequest();

        await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

        // Lets OpenIddict honour post_logout_redirect_uri back to the SPA.
        return SignOut(
            authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            properties: new AuthenticationProperties { RedirectUri = request?.PostLogoutRedirectUri ?? "/" });
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

    // Decides which token each claim is copied into. Identity/profile claims also flow to the
    // id_token when the matching scope was granted; everything else stays in the access token.
    private static IEnumerable<string> GetDestinations(Claim claim, ClaimsPrincipal principal)
    {
        switch (claim.Type)
        {
            case Claims.Name:
                yield return Destinations.AccessToken;
                if (principal.HasScope(Scopes.Profile))
                {
                    yield return Destinations.IdentityToken;
                }

                yield break;

            case Claims.Email:
                yield return Destinations.AccessToken;
                if (principal.HasScope(Scopes.Email))
                {
                    yield return Destinations.IdentityToken;
                }

                yield break;

            case Claims.Role:
                yield return Destinations.AccessToken;
                if (principal.HasScope(Scopes.Roles))
                {
                    yield return Destinations.IdentityToken;
                }

                yield break;

            // Never expose the security stamp.
            case "AspNet.Identity.SecurityStamp":
                yield break;

            default:
                yield return Destinations.AccessToken;
                yield break;
        }
    }
}
