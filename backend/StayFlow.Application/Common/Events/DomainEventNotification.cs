using MediatR;
using StayFlow.Domain.Common;

namespace StayFlow.Application.Common.Events;

/// <summary>
/// Adapts a pure-domain <see cref="IDomainEvent"/> to a MediatR notification so the
/// persistence layer can publish it without the domain depending on MediatR. Handlers
/// implement <c>INotificationHandler&lt;DomainEventNotification&lt;TDomainEvent&gt;&gt;</c>.
/// </summary>
public sealed record DomainEventNotification<TDomainEvent>(TDomainEvent DomainEvent) : INotification
    where TDomainEvent : IDomainEvent;
