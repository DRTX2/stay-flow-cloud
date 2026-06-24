using Microsoft.AspNetCore.Identity;

namespace StayFlow.Persistence.Identity;

/// <summary>Application role. Permissions are stored as role claims and copied onto tokens.</summary>
public sealed class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole()
    {
    }

    public ApplicationRole(string roleName) : base(roleName)
    {
    }

    public string? Description { get; set; }
}
