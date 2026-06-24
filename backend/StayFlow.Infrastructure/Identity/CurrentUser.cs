using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using OpenIddict.Abstractions;
using StayFlow.Application.Common.Abstractions;

namespace StayFlow.Infrastructure.Identity;

/// <summary>Projects the current request's authenticated principal from its claims.</summary>
public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var value = Principal?.FindFirstValue(OpenIddictConstants.Claims.Subject)
                ?? Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public string? UserName =>
        Principal?.FindFirstValue(OpenIddictConstants.Claims.Name)
        ?? Principal?.FindFirstValue(ClaimTypes.Name);

    public Guid? TenantId
    {
        get
        {
            var value = Principal?.FindFirstValue("tenant_id");
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public IReadOnlySet<string> Permissions =>
        Principal?.FindAll("permission").Select(c => c.Value).ToHashSet(StringComparer.Ordinal)
        ?? new HashSet<string>(StringComparer.Ordinal);
}
