using StayFlow.Domain.Common;
using StayFlow.Domain.Services;

namespace StayFlow.Domain.Billing;

/// <summary>
/// A posted ancillary charge against a reservation (e.g. "2x Breakfast"). The unit price is
/// captured at posting time so later catalogue price changes do not rewrite history.
/// </summary>
public sealed class ReservationCharge : TenantEntity
{
    private ReservationCharge()
    {
    }

    private ReservationCharge(Guid reservationId, Guid serviceItemId, string description, int quantity, decimal unitPrice)
    {
        ReservationId = reservationId;
        ServiceItemId = serviceItemId;
        Description = description;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public Guid ReservationId { get; private set; }

    public Guid ServiceItemId { get; private set; }

    public string Description { get; private set; } = string.Empty;

    public int Quantity { get; private set; }

    public decimal UnitPrice { get; private set; }

    public decimal Amount => UnitPrice * Quantity;

    public static ReservationCharge Create(Guid reservationId, ServiceItem service, int quantity)
    {
        ArgumentNullException.ThrowIfNull(service);

        if (reservationId == Guid.Empty)
        {
            throw new DomainException("Charge must reference a reservation.");
        }

        if (!service.IsActive)
        {
            throw new DomainException($"Service '{service.Name}' is not active and cannot be charged.");
        }

        if (quantity < 1)
        {
            throw new DomainException("Charge quantity must be at least 1.");
        }

        return new ReservationCharge(reservationId, service.Id, service.Name, quantity, service.Price);
    }
}
