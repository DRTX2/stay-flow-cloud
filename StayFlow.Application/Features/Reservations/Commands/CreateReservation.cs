using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Application.Common.Exceptions;
using StayFlow.Application.Pricing;
using StayFlow.Domain.Guests;
using StayFlow.Domain.Reservations;
using StayFlow.Domain.Rooms;
using ValidationException = StayFlow.Application.Common.Exceptions.ValidationException;

namespace StayFlow.Application.Features.Reservations.Commands;

public sealed record CreateReservationCommand(
    Guid RoomId,
    Guid GuestId,
    DateOnly CheckIn,
    DateOnly CheckOut,
    int NumberOfGuests) : IRequest<ReservationDto>;

public sealed class CreateReservationValidator : AbstractValidator<CreateReservationCommand>
{
    public CreateReservationValidator()
    {
        RuleFor(x => x.RoomId).NotEmpty();
        RuleFor(x => x.GuestId).NotEmpty();
        RuleFor(x => x.NumberOfGuests).GreaterThanOrEqualTo(1);
        RuleFor(x => x.CheckOut)
            .GreaterThan(x => x.CheckIn)
            .WithMessage("Check-out must be after check-in.");
    }
}

public sealed class CreateReservationHandler(IApplicationDbContext dbContext, IPricingService pricingService)
    : IRequestHandler<CreateReservationCommand, ReservationDto>
{
    public async Task<ReservationDto> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
    {
        var room = await dbContext.Rooms.FirstOrDefaultAsync(r => r.Id == request.RoomId, cancellationToken)
            ?? throw new NotFoundException(nameof(Room), request.RoomId);

        if (!room.IsBookable)
        {
            throw new ValidationException(
            [
                new ValidationFailure(nameof(request.RoomId), $"Room '{room.Number}' is not available for booking (status: {room.Status})."),
            ]);
        }

        if (request.NumberOfGuests > room.Capacity)
        {
            throw new ValidationException(
            [
                new ValidationFailure(nameof(request.NumberOfGuests), $"Room '{room.Number}' holds at most {room.Capacity} guests."),
            ]);
        }

        var guestExists = await dbContext.Guests.AnyAsync(g => g.Id == request.GuestId, cancellationToken);
        if (!guestExists)
        {
            throw new NotFoundException(nameof(Guest), request.GuestId);
        }

        var period = DateRange.Create(request.CheckIn, request.CheckOut);

        var overlaps = await ReservationAvailability.HasOverlapAsync(
            dbContext, request.RoomId, request.CheckIn, request.CheckOut, cancellationToken);
        if (overlaps)
        {
            throw new ValidationException(
            [
                new ValidationFailure(nameof(request.RoomId), "The room is already booked for the selected dates."),
            ]);
        }

        var occupancy = await ReservationAvailability.OccupancyAsync(
            dbContext, request.CheckIn, request.CheckOut, cancellationToken);

        var quote = pricingService.Quote(new PricingRequest(
            room.BasePrice,
            request.CheckIn,
            request.CheckOut,
            occupancy,
            request.NumberOfGuests));

        var reservation = Reservation.Create(request.RoomId, request.GuestId, period, request.NumberOfGuests, quote.TotalPrice);
        dbContext.Reservations.Add(reservation);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ReservationDto.FromEntity(reservation);
    }
}
