namespace StayFlow.Persistence.Identity;

/// <summary>A short-lived, tenant-bound credential used to explicitly claim a guest profile.</summary>
public sealed class PortalGuestInvitation
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid GuestId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAtUtc { get; set; }
    public DateTimeOffset? RedeemedAtUtc { get; set; }
    public Guid? RedeemedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}
