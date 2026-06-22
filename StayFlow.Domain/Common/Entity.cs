namespace StayFlow.Domain.Common;

/// <summary>
/// Base type for all domain entities. Provides identity and a domain-event buffer
/// that the persistence layer drains and dispatches after a successful commit.
/// </summary>
public abstract class Entity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected Entity(Guid id) => Id = id;

    protected Entity() => Id = Guid.CreateVersion7();

    public Guid Id { get; protected set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
