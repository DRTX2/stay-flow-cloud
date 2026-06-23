using FluentAssertions;
using StayFlow.Domain.Common;
using StayFlow.Domain.Reservations;
using StayFlow.Domain.Reservations.Events;

namespace StayFlow.UnitTests.Domain;

public sealed class ReservationTests
{
    private static readonly DateRange Period = DateRange.Create(new DateOnly(2026, 5, 1), new DateOnly(2026, 5, 4));

    private static Reservation NewReservation() =>
        Reservation.Create(Guid.NewGuid(), Guid.NewGuid(), Period, numberOfGuests: 2, totalPrice: 300m);

    [Fact]
    public void Create_Valid_StartsPendingAndRaisesEvent()
    {
        var reservation = NewReservation();

        reservation.Status.Should().Be(ReservationStatus.Pending);
        reservation.ConfirmationCode.Should().StartWith("SF-");
        reservation.DomainEvents.Should().ContainSingle(e => e is ReservationCreatedEvent);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void Create_NonPositiveGuests_Throws(int guests)
    {
        var act = () => Reservation.Create(Guid.NewGuid(), Guid.NewGuid(), Period, guests, 100m);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_NegativePrice_Throws()
    {
        var act = () => Reservation.Create(Guid.NewGuid(), Guid.NewGuid(), Period, 1, -1m);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Lifecycle_HappyPath_TransitionsThroughEachState()
    {
        var reservation = NewReservation();

        reservation.Confirm();
        reservation.Status.Should().Be(ReservationStatus.Confirmed);

        reservation.CheckIn();
        reservation.Status.Should().Be(ReservationStatus.CheckedIn);

        reservation.CheckOut();
        reservation.Status.Should().Be(ReservationStatus.CheckedOut);

        reservation.DomainEvents.Should().Contain(e => e is ReservationConfirmedEvent)
            .And.Contain(e => e is ReservationCheckedInEvent)
            .And.Contain(e => e is ReservationCheckedOutEvent);
    }

    [Fact]
    public void CheckIn_WithoutConfirm_Throws()
    {
        var reservation = NewReservation();

        reservation.Invoking(r => r.CheckIn()).Should().Throw<DomainException>();
    }

    [Fact]
    public void CheckOut_WithoutCheckIn_Throws()
    {
        var reservation = NewReservation();
        reservation.Confirm();

        reservation.Invoking(r => r.CheckOut()).Should().Throw<DomainException>();
    }

    [Fact]
    public void Cancel_AfterCheckIn_Throws()
    {
        var reservation = NewReservation();
        reservation.Confirm();
        reservation.CheckIn();

        reservation.Invoking(r => r.Cancel("late")).Should().Throw<DomainException>();
    }

    [Fact]
    public void Cancel_FromConfirmed_SetsCancelledWithReason()
    {
        var reservation = NewReservation();
        reservation.Confirm();

        reservation.Cancel("guest request");

        reservation.Status.Should().Be(ReservationStatus.Cancelled);
        reservation.CancellationReason.Should().Be("guest request");
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_IsIdempotent()
    {
        var reservation = NewReservation();
        reservation.Cancel();

        reservation.Invoking(r => r.Cancel()).Should().NotThrow();
        reservation.Status.Should().Be(ReservationStatus.Cancelled);
    }

    [Fact]
    public void MarkNoShow_OnlyFromConfirmed()
    {
        var pending = NewReservation();
        pending.Invoking(r => r.MarkNoShow()).Should().Throw<DomainException>();

        var confirmed = NewReservation();
        confirmed.Confirm();
        confirmed.MarkNoShow();
        confirmed.Status.Should().Be(ReservationStatus.NoShow);
    }
}
