using StayFlow.Domain.Common;

namespace StayFlow.Domain.Rooms;

/// <summary>
/// A category of room (e.g. Standard, Deluxe, Suite) carrying the base nightly rate and
/// maximum occupancy. Rooms reference a room type; the dynamic pricing engine adjusts the
/// base rate per stay.
/// </summary>
public sealed class RoomType : TenantEntity
{
    private RoomType()
    {
    }

    private RoomType(string name, string? description, decimal baseRate, int maxOccupancy)
    {
        Name = name;
        Description = description;
        BaseRate = baseRate;
        MaxOccupancy = maxOccupancy;
    }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    /// <summary>Base nightly rate in the tenant's default currency, before dynamic pricing.</summary>
    public decimal BaseRate { get; private set; }

    public int MaxOccupancy { get; private set; }

    public static RoomType Create(string name, decimal baseRate, int maxOccupancy, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Room type name is required.");
        }

        if (baseRate < 0)
        {
            throw new DomainException("Base rate cannot be negative.");
        }

        if (maxOccupancy < 1)
        {
            throw new DomainException("Max occupancy must be at least 1.");
        }

        return new RoomType(name.Trim(), description?.Trim(), baseRate, maxOccupancy);
    }

    public void Update(string name, decimal baseRate, int maxOccupancy, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Room type name is required.");
        }

        if (baseRate < 0)
        {
            throw new DomainException("Base rate cannot be negative.");
        }

        if (maxOccupancy < 1)
        {
            throw new DomainException("Max occupancy must be at least 1.");
        }

        Name = name.Trim();
        Description = description?.Trim();
        BaseRate = baseRate;
        MaxOccupancy = maxOccupancy;
    }
}
