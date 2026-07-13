using StayFlow.Domain.Common;

namespace StayFlow.Domain.Rooms;

/// <summary>
/// A physical bookable unit belonging to a tenant. Carries its own base price (seeded from
/// the room type but individually adjustable) and an operational status.
/// </summary>
public sealed class Room : TenantEntity
{
    private Room()
    {
    }

    private Room(string number, Guid roomTypeId, decimal basePrice, int capacity, int floor)
    {
        Number = number;
        RoomTypeId = roomTypeId;
        BasePrice = basePrice;
        Capacity = capacity;
        Floor = floor;
        Status = RoomStatus.Available;
        CleaningStatus = RoomCleaningStatus.Clean;
    }

    public string Number { get; private set; } = string.Empty;

    public Guid RoomTypeId { get; private set; }

    /// <summary>Base nightly price in the tenant's default currency.</summary>
    public decimal BasePrice { get; private set; }

    public int Capacity { get; private set; }

    public int Floor { get; private set; }

    public RoomStatus Status { get; private set; }

    public RoomCleaningStatus CleaningStatus { get; private set; }

    public static Room Create(string number, Guid roomTypeId, decimal basePrice, int capacity, int floor = 0)
    {
        if (string.IsNullOrWhiteSpace(number))
        {
            throw new DomainException("Room number is required.");
        }

        if (roomTypeId == Guid.Empty)
        {
            throw new DomainException("Room must reference a room type.");
        }

        if (basePrice < 0)
        {
            throw new DomainException("Base price cannot be negative.");
        }

        if (capacity < 1)
        {
            throw new DomainException("Capacity must be at least 1.");
        }

        return new Room(number.Trim(), roomTypeId, basePrice, capacity, floor);
    }

    public void ChangePrice(decimal newPrice)
    {
        if (newPrice < 0)
        {
            throw new DomainException("Base price cannot be negative.");
        }

        BasePrice = newPrice;
    }

    public void MarkOccupied() => Status = RoomStatus.Occupied;

    public void PutUnderMaintenance() => Status = RoomStatus.Maintenance;

    public void ReturnToService() => Status = RoomStatus.Available;

    public void MarkOutOfService() => Status = RoomStatus.OutOfService;

    public void UpdateCleaningStatus(RoomCleaningStatus status) => CleaningStatus = status;

    /// <summary>
    /// Whether the room is operationally eligible for a reservation. Occupied is intentionally
    /// bookable because date overlap, rather than today's occupancy, determines future availability.
    /// </summary>
    public bool IsBookable => Status is not RoomStatus.Maintenance and not RoomStatus.OutOfService;
}
