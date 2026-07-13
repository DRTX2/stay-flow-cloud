using FluentAssertions;
using StayFlow.Infrastructure.Messaging;
using StayFlow.NotificationService.Consumers;

namespace StayFlow.UnitTests.Notifications;

public sealed class NotificationEventMappingTests
{
    [Theory]
    [InlineData("StayFlow.Domain.Reservations.Events.ReservationCreatedEvent")]
    [InlineData("StayFlow.Domain.Maintenance.Events.WorkOrderResolvedEvent")]
    [InlineData("InvoicePaidEvent")]
    public void InAppMapping_MatchesActualEventNames(string eventType)
    {
        DomainEventOccurredConsumer.IsNotifiableEvent(eventType).Should().BeTrue();
    }

    [Theory]
    [InlineData("ReservationCreated")]
    [InlineData("GuestRegisteredEvent")]
    [InlineData("ReservationCreatedEventUnexpected")]
    public void InAppMapping_RejectsUnboundedNames(string eventType)
    {
        DomainEventOccurredConsumer.IsNotifiableEvent(eventType).Should().BeFalse();
    }

    [Fact]
    public void ExternalPrototype_MatchesEventSuffix()
    {
        DomainEventNotificationConsumer.ResolveChannel(
                "StayFlow.Domain.Reservations.Events.ReservationCreatedEvent")
            .Should().Be("Email");
        DomainEventNotificationConsumer.ResolveChannel("ReservationCreated").Should().BeNull();
    }
}
