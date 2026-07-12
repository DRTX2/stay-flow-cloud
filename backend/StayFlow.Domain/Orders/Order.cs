using StayFlow.Domain.Common;
using StayFlow.Domain.Orders.Events;

namespace StayFlow.Domain.Orders;

public sealed class Order : TenantEntity
{
    private readonly List<OrderLineItem> _items = [];

    private Order()
    {
    }

    private Order(Guid reservationId, string? notes)
    {
        ReservationId = reservationId;
        Notes = notes;
        Status = OrderStatus.Pending;
    }

    public Guid ReservationId { get; private set; }
    
    public OrderStatus Status { get; private set; }
    
    public string? Notes { get; private set; }
    
    public DateTimeOffset? DeliveredAtUtc { get; private set; }

    public IReadOnlyCollection<OrderLineItem> Items => _items.AsReadOnly();

    public decimal TotalAmount => _items.Sum(i => i.Total);

    public static Order Create(Guid reservationId, string? notes = null)
    {
        if (reservationId == Guid.Empty)
        {
            throw new DomainException("ReservationId is required.");
        }
        return new Order(reservationId, notes?.Trim());
    }

    public void AddItem(Guid serviceItemId, string serviceName, int quantity, decimal unitPrice)
    {
        if (Status != OrderStatus.Pending && Status != OrderStatus.Preparing)
        {
            throw new DomainException("Cannot add items to an order that is already delivered or cancelled.");
        }
            
        if (quantity <= 0)
        {
            throw new DomainException("Quantity must be greater than zero.");
        }
        if (unitPrice < 0)
        {
            throw new DomainException("Unit price cannot be negative.");
        }

        _items.Add(new OrderLineItem(Id, serviceItemId, serviceName, quantity, unitPrice));
    }

    public void Place()
    {
        if (_items.Count == 0)
        {
            throw new DomainException("Order must have at least one item to be placed.");
        }
            
        RaiseDomainEvent(new OrderPlacedEvent(Id, TenantId, ReservationId, TotalAmount));
    }

    public void MarkPreparing()
    {
        if (Status != OrderStatus.Pending)
        {
            throw new DomainException($"Cannot mark order as preparing from {Status} status.");
        }
            
        Status = OrderStatus.Preparing;
    }

    public void MarkDelivered()
    {
        if (Status == OrderStatus.Delivered)
        {
            return;
        }

        if (Status != OrderStatus.Preparing)
        {
            throw new DomainException($"Cannot mark order as delivered from {Status} status.");
        }
            
        Status = OrderStatus.Delivered;
        DeliveredAtUtc = DateTimeOffset.UtcNow;
        
        RaiseDomainEvent(new OrderDeliveredEvent(Id, TenantId));
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Delivered)
        {
            throw new DomainException("Cannot cancel a delivered order.");
        }
            
        Status = OrderStatus.Cancelled;
    }
}
