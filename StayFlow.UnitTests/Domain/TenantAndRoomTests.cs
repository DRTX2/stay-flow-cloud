using FluentAssertions;
using StayFlow.Domain.Common;
using StayFlow.Domain.Rooms;
using StayFlow.Domain.Tenants;

namespace StayFlow.UnitTests.Domain;

public sealed class TenantTests
{
    [Fact]
    public void Create_NormalisesSlugAndCurrency_AndDefaultsPlan()
    {
        var tenant = Tenant.Create("  Grand Hotel ", "Grand-Hotel", PropertyType.Hotel, "usd");

        tenant.Name.Should().Be("Grand Hotel");
        tenant.Slug.Should().Be("grand-hotel");
        tenant.DefaultCurrency.Should().Be("USD");
        tenant.Plan.Should().Be(SubscriptionPlan.Basic);
        tenant.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "slug", "USD")]
    [InlineData("Name", "", "USD")]
    [InlineData("Name", "slug", "US")]   // currency must be 3 letters
    public void Create_InvalidInput_Throws(string name, string slug, string currency)
    {
        var act = () => Tenant.Create(name, slug, PropertyType.Hotel, currency);

        act.Should().Throw<DomainException>();
    }
}

public sealed class RoomTests
{
    [Fact]
    public void Create_StartsAvailableAndBookable()
    {
        var room = Room.Create("101", Guid.NewGuid(), 90m, 2);

        room.Status.Should().Be(RoomStatus.Available);
        room.IsBookable.Should().BeTrue();
    }

    [Fact]
    public void PutUnderMaintenance_MakesRoomNotBookable()
    {
        var room = Room.Create("101", Guid.NewGuid(), 90m, 2);

        room.PutUnderMaintenance();

        room.IsBookable.Should().BeFalse();
    }

    [Fact]
    public void ChangePrice_Negative_Throws()
    {
        var room = Room.Create("101", Guid.NewGuid(), 90m, 2);

        room.Invoking(r => r.ChangePrice(-5m)).Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_ZeroCapacity_Throws()
    {
        var act = () => Room.Create("101", Guid.NewGuid(), 90m, 0);

        act.Should().Throw<DomainException>();
    }
}
