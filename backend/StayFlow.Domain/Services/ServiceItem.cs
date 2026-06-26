using StayFlow.Domain.Common;

namespace StayFlow.Domain.Services;

/// <summary>
/// An ancillary service offered by a tenant (breakfast, spa, airport transfer, …). Sold on top
/// of the room stay and billed through the reservation's invoice.
/// </summary>
public sealed class ServiceItem : TenantEntity
{
    private ServiceItem()
    {
    }

    private ServiceItem(string name, decimal price, ServiceCategory category, string? description)
    {
        Name = name;
        Price = price;
        Category = category;
        Description = description;
        IsActive = true;
    }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public decimal Price { get; private set; }

    public ServiceCategory Category { get; private set; }

    public bool IsActive { get; private set; }

    public static ServiceItem Create(string name, decimal price, ServiceCategory category, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Service name is required.");
        }

        if (price < 0)
        {
            throw new DomainException("Service price cannot be negative.");
        }

        return new ServiceItem(name.Trim(), price, category, description?.Trim());
    }

    public void ChangePrice(decimal newPrice)
    {
        if (newPrice < 0)
        {
            throw new DomainException("Service price cannot be negative.");
        }

        Price = newPrice;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    public void Update(string name, decimal price, ServiceCategory category, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Service name is required.");
        }

        if (price < 0)
        {
            throw new DomainException("Service price cannot be negative.");
        }

        Name = name.Trim();
        Price = price;
        Category = category;
        Description = description?.Trim();
    }
}
