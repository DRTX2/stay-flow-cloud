using FluentAssertions;
using StayFlow.Domain.Common;
using StayFlow.Domain.Orders;

namespace StayFlow.UnitTests.Domain;

public sealed class OrderTests
{
    [Fact]
    public void Order_RequiresPreparingBeforeDelivery()
    {
        var order = Order.Create(Guid.NewGuid());
        order.AddItem(Guid.NewGuid(), "Breakfast", 1, 20m);
        order.Place();

        var deliverPending = () => order.MarkDelivered();
        deliverPending.Should().Throw<DomainException>();

        order.MarkPreparing();
        order.MarkDelivered();

        order.Status.Should().Be(OrderStatus.Delivered);
        order.DeliveredAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void DeliveredOrder_CannotBeCancelled()
    {
        var order = Order.Create(Guid.NewGuid());
        order.AddItem(Guid.NewGuid(), "Dinner", 2, 30m);
        order.MarkPreparing();
        order.MarkDelivered();

        var cancel = () => order.Cancel();

        cancel.Should().Throw<DomainException>();
    }
}
