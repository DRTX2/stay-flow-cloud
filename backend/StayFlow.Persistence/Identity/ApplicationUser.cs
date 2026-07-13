using Microsoft.AspNetCore.Identity;

namespace StayFlow.Persistence.Identity;

/// <summary>
/// Application user backed by ASP.NET Identity. Each user belongs to a tenant; the tenant id
/// is emitted as a token claim and drives multi-tenant isolation for authenticated requests.
/// </summary>
public sealed class ApplicationUser : IdentityUser<Guid>
{
    public Guid TenantId { get; set; }

    /// <summary>The explicitly claimed guest profile. Never inferred from an email address.</summary>
    public Guid? GuestId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
