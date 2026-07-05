using System.Security.Claims;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StayFlow.Application.Common.Authorization;
using StayFlow.Persistence.Identity;

namespace StayFlow.Api.Controllers;

/// <summary>
/// Interactive login for the Authorization Code flow: a minimal email/password form plus external
/// (social) provider sign-in. Establishes the Identity application cookie that
/// <see cref="AuthorizeController"/> relies on. A real deployment would render these via the SPA.
/// </summary>
public sealed class AccountController(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    IAuthenticationSchemeProvider schemeProvider) : ControllerBase
{
    private static readonly string[] SupportedExternalProviders = ["Google", "Microsoft", "GitHub"];

    [HttpGet("~/account/login")]
    public async Task<IActionResult> Login(string? returnUrl = null)
        => Content(await BuildLoginPageAsync(returnUrl, error: null), "text/html");

    [HttpPost("~/account/login")]
    [IgnoreAntiforgeryToken]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> Login([FromForm] string email, [FromForm] string password, [FromForm] string? returnUrl = null)
    {
        var result = await signInManager.PasswordSignInAsync(email, password, isPersistent: false, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            return Content(await BuildLoginPageAsync(returnUrl, error: "Invalid email or password."), "text/html");
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

    [HttpGet("~/account/external")]
    public IActionResult External(string provider, string? returnUrl = null)
    {
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
            return SafeRedirect("/account/login");
        }

        // Already linked: sign in directly.
        var signIn = await signInManager.ExternalLoginSignInAsync(
            info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
        if (signIn.Succeeded)
        {
            return SafeRedirect(returnUrl);
        }

        // First time with this provider: provision a tenant-less Customer and link the login.
        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrWhiteSpace(email))
        {
            return SafeRedirect("/account/login");
        }

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
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
                return SafeRedirect("/account/login");
            }

            await userManager.AddToRoleAsync(user, Roles.Customer);
        }

        await userManager.AddLoginAsync(user, info);
        await signInManager.SignInAsync(user, isPersistent: false);
        return SafeRedirect(returnUrl);
    }

    // Only ever redirect to local URLs, to avoid open-redirect abuse via returnUrl.
    private LocalRedirectResult SafeRedirect(string? returnUrl)
        => LocalRedirect(!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ? returnUrl : "/");

    private async Task<string> BuildLoginPageAsync(string? returnUrl, string? error)
    {
        var registered = (await schemeProvider.GetAllSchemesAsync())
            .Select(scheme => scheme.Name)
            .Where(name => SupportedExternalProviders.Contains(name))
            .ToList();

        var encodedReturnUrl = HttpUtility.HtmlAttributeEncode(returnUrl ?? "/");

        var builder = new StringBuilder();
        builder.Append("<!doctype html><html><head><meta charset=\"utf-8\"><title>StayFlow — Sign in</title></head><body>");
        builder.Append("<h1>StayFlow sign in</h1>");

        if (error is not null)
        {
            builder.Append("<p style=\"color:red\">").Append(HttpUtility.HtmlEncode(error)).Append("</p>");
        }

        builder.Append("<form method=\"post\" action=\"/account/login\">");
        builder.Append("<input type=\"hidden\" name=\"returnUrl\" value=\"").Append(encodedReturnUrl).Append("\"/>");
        builder.Append("<p><label>Email <input name=\"email\" type=\"email\" required/></label></p>");
        builder.Append("<p><label>Password <input name=\"password\" type=\"password\" required/></label></p>");
        builder.Append("<button type=\"submit\">Sign in</button></form>");

        if (registered.Count > 0)
        {
            builder.Append("<h2>Or continue with</h2><ul>");
            foreach (var provider in registered)
            {
                var href = $"/account/external?provider={HttpUtility.UrlEncode(provider)}&returnUrl={HttpUtility.UrlEncode(returnUrl ?? "/")}";
                builder.Append("<li><a href=\"").Append(href).Append("\">").Append(provider).Append("</a></li>");
            }

            builder.Append("</ul>");
        }

        builder.Append("</body></html>");
        return builder.ToString();
    }
}
