namespace StayFlow.Domain.Common;

/// <summary>
/// Marker for domain events. Kept free of infrastructure concerns (no MediatR
/// dependency) so the domain stays pure; the Application layer adapts these to
/// notifications for dispatch.
/// </summary>
public interface IDomainEvent
{
    DateTimeOffset OccurredOnUtc { get; }
}

public abstract record DomainEvent : IDomainEvent
{
    public DateTimeOffset OccurredOnUtc { get; init; } = DateTimeOffset.UtcNow;
}
