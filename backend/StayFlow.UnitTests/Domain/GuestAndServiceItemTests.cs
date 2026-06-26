using FluentAssertions;
using StayFlow.Domain.Common;
using StayFlow.Domain.Guests;
using StayFlow.Domain.Services;

namespace StayFlow.UnitTests.Domain;

public sealed class GuestAndServiceItemTests
{
    [Fact]
    public void Guest_UpdateProfile_OverwritesFieldsAndNormalizesEmail()
    {
        var guest = Guest.Create("Jane", "Doe", "jane@example.com");

        guest.UpdateProfile("  John ", " Smith ", "JOHN@Example.com", " +1 555 ", " AB123 ");

        guest.FirstName.Should().Be("John");
        guest.LastName.Should().Be("Smith");
        guest.Email.Should().Be("john@example.com");
        guest.Phone.Should().Be("+1 555");
        guest.DocumentNumber.Should().Be("AB123");
        guest.FullName.Should().Be("John Smith");
    }

    [Theory]
    [InlineData("", "Smith", "john@example.com")]
    [InlineData("John", "", "john@example.com")]
    [InlineData("John", "Smith", "not-an-email")]
    public void Guest_UpdateProfile_Invalid_Throws(string first, string last, string email)
    {
        var guest = Guest.Create("Jane", "Doe", "jane@example.com");

        var act = () => guest.UpdateProfile(first, last, email, null, null);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ServiceItem_Update_OverwritesFields()
    {
        var service = ServiceItem.Create("Breakfast", 12m, ServiceCategory.FoodAndBeverage);

        service.Update(" Spa Day ", 80m, ServiceCategory.Spa, "  Relaxing  ");

        service.Name.Should().Be("Spa Day");
        service.Price.Should().Be(80m);
        service.Category.Should().Be(ServiceCategory.Spa);
        service.Description.Should().Be("Relaxing");
    }

    [Fact]
    public void ServiceItem_Update_NegativePrice_Throws()
    {
        var service = ServiceItem.Create("Breakfast", 12m, ServiceCategory.FoodAndBeverage);

        var act = () => service.Update("Breakfast", -1m, ServiceCategory.FoodAndBeverage, null);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ServiceItem_Update_EmptyName_Throws()
    {
        var service = ServiceItem.Create("Breakfast", 12m, ServiceCategory.FoodAndBeverage);

        var act = () => service.Update("  ", 12m, ServiceCategory.FoodAndBeverage, null);

        act.Should().Throw<DomainException>();
    }
}
