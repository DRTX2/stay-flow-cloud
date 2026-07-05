using StayFlow.Domain.Common;

namespace StayFlow.Domain.Orders;

public sealed class OrderLineItem : Entity
{
    private OrderLineItem()
    {
    }

    internal OrderLineItem(Guid orderId, Guid serviceItemId, string serviceName, int quantity, decimal unitPrice)
    {
        OrderId = orderId;
        ServiceItemId = serviceItemId;
        ServiceName = serviceName;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public Guid OrderId { get; private set; }
    
    public Guid ServiceItemId { get; private set; }
    
    public string ServiceName { get; private set; } = string.Empty;
    
    public int Quantity { get; private set; }
    
    public decimal UnitPrice { get; private set; }

    public decimal Total => Quantity * UnitPrice;
}
