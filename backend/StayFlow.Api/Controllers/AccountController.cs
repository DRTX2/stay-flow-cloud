using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StayFlow.Application.Common.Authorization;
using StayFlow.Persistence.Identity;

namespace StayFlow.Api.Controllers;

/// <summary>
/// Handles the credential-exchange step of the Authorization Code + PKCE flow.
///
/// The interactive login UI lives entirely in the Next.js frontend
/// (GET /auth/signin). That page POSTs email/password here; on success we write
/// the Identity application cookie and redirect back to /connect/authorize so
/// OpenIddict can complete the code exchange.
///
/// External (social) provider sign-ins are also brokered here.
/// </summary>
public sealed class AccountController(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    IAuthenticationSchemeProvider schemeProvider) : ControllerBase
{
    private static readonly string[] SupportedExternalProviders = ["Google", "Microsoft", "Facebook", "GitHub"];


    /// <summary>
    /// Accepts email + password, validates credentials, writes the Identity cookie,
    /// then redirects back to the original ReturnUrl (which is the /connect/authorize callback).
    /// </summary>
    [HttpPost("~/account/login")]
    [IgnoreAntiforgeryToken]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> Login(
        [FromForm] string email,
        [FromForm] string password,
        [FromForm] string? returnUrl = null)
    {
        var result = await signInManager.PasswordSignInAsync(
            email, password, isPersistent: false, lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            var config = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var loginFallback = config["Authentication:FrontendLoginUrl"] ?? "http://localhost:3000/signin";
            var redirect = $"{loginFallback}?error=invalid_credentials&ReturnUrl={Uri.EscapeDataString(returnUrl ?? "/")}";
            return Redirect(redirect);
        }

        return SafeRedirect(returnUrl);
    }

    [HttpPost("~/account/logout")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
        return SafeRedirect(returnUrl: null);
    }

    [HttpGet("~/account/external/providers")]
    public async Task<ActionResult<IReadOnlyList<ExternalProviderDto>>> ExternalProviders()
    {
        var schemes = await schemeProvider.GetAllSchemesAsync();
        var configured = schemes.Select(scheme => scheme.Name).ToHashSet(StringComparer.Ordinal);
        return Ok(SupportedExternalProviders
            .Where(configured.Contains)
            .Select(provider => new ExternalProviderDto(provider, provider == "Microsoft" ? "Microsoft" : provider))
            .ToList());
    }

    [HttpGet("~/account/external")]
    public async Task<IActionResult> External(string provider, string? returnUrl = null)
    {
        var scheme = await schemeProvider.GetSchemeAsync(provider);
        if (!SupportedExternalProviders.Contains(provider, StringComparer.Ordinal) || scheme is null)
        {
            return BadRequest("The requested external sign-in provider is unavailable.");
        }

        var redirectUrl = Url.Action(nameof(ExternalCallback), "Account", new { returnUrl });
        var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(properties, provider);
    }

    [HttpGet("~/account/external/callback")]
    public async Task<IActionResult> ExternalCallback(string? returnUrl = null)
    {
        var info = await signInManager.GetExternalLoginInfoAsync();
        if (info is null)
        {
            return SafeRedirect("/");
        }

        // Already linked: sign in directly.
        var signIn = await signInManager.ExternalLoginSignInAsync(
            info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: false);
        if (signIn.Succeeded)
        {
            return SafeRedirect(returnUrl);
        }

        // First time with this provider: provision a tenant-less Customer and link the login.
        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrWhiteSpace(email))
        {
            return SafeRedirect("/");
        }

        var user = await userManager.FindByEmailAsync(email);
        if (user is not null)
        {
            // Never attach a new social identity to an existing account based only on an email
            // claim. Account linking requires an authenticated, explicit flow.
            return SafeRedirect("/");
        }

        user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = info.Principal.FindFirstValue(ClaimTypes.Name) ?? email,
            TenantId = Guid.Empty,
        };

        var created = await userManager.CreateAsync(user);
        if (!created.Succeeded)
        {
            return SafeRedirect("/");
        }

        var roleAdded = await userManager.AddToRoleAsync(user, Roles.Customer);
        if (!roleAdded.Succeeded)
        {
            await userManager.DeleteAsync(user);
            return SafeRedirect("/");
        }

        var linked = await userManager.AddLoginAsync(user, info);
        if (!linked.Succeeded)
        {
            await userManager.DeleteAsync(user);
            return SafeRedirect("/");
        }
        await signInManager.SignInAsync(user, isPersistent: false);
        return SafeRedirect(returnUrl);
    }

    // Only ever redirect to local URLs or the configured frontend, to avoid open-redirect abuse.
    private LocalRedirectResult SafeRedirect(string? returnUrl)
        => LocalRedirect(!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ? returnUrl : "/");

    public sealed record ExternalProviderDto(string Scheme, string DisplayName);
}
