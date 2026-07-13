namespace StayFlow.Application.Common.Abstractions;

/// <summary>The authenticated principal for the current request, projected from its claims.</summary>
public interface ICurrentUser
{
    Guid? UserId { get; }

    string? UserName { get; }

    Guid? TenantId { get; }

    Guid? GuestId { get; }

    bool IsAuthenticated { get; }

    IReadOnlySet<string> Permissions { get; }

    bool HasPermission(string permission) => Permissions.Contains(permission);
}
