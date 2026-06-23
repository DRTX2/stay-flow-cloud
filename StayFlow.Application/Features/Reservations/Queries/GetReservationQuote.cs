using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Application.Common.Exceptions;
using StayFlow.Application.Pricing;
using StayFlow.Domain.Rooms;

namespace StayFlow.Application.Features.Reservations.Queries;

/// <summary>Previews the price of a prospective stay without creating a reservation.</summary>
public sealed record GetReservationQuoteQuery(
    Guid RoomId,
    DateOnly CheckIn,
    DateOnly CheckOut,
    int NumberOfGuests) : IRequest<QuoteDto>;

public sealed record QuoteDto(
    Guid RoomId,
    DateOnly CheckIn,
    DateOnly CheckOut,
    int Nights,
    decimal TotalPrice,
    decimal AverageNightlyRate,
    IReadOnlyList<string> Adjustments);

public sealed class GetReservationQuoteValidator : AbstractValidator<GetReservationQuoteQuery>
{
    public GetReservationQuoteValidator()
    {
        RuleFor(x => x.RoomId).NotEmpty();
        RuleFor(x => x.NumberOfGuests).GreaterThanOrEqualTo(1);
        RuleFor(x => x.CheckOut).GreaterThan(x => x.CheckIn).WithMessage("Check-out must be after check-in.");
    }
}

public sealed class GetReservationQuoteHandler(IApplicationDbContext dbContext, IPricingService pricingService)
    : IRequestHandler<GetReservationQuoteQuery, QuoteDto>
{
    public async Task<QuoteDto> Handle(GetReservationQuoteQuery request, CancellationToken cancellationToken)
    {
        var room = await dbContext.Rooms.AsNoTracking().FirstOrDefaultAsync(r => r.Id == request.RoomId, cancellationToken)
            ?? throw new NotFoundException(nameof(Room), request.RoomId);

        var occupancy = await Reservations.ReservationAvailability.OccupancyAsync(
            dbContext, request.CheckIn, request.CheckOut, cancellationToken);

        var quote = pricingService.Quote(new PricingRequest(
            room.BasePrice,
            request.CheckIn,
            request.CheckOut,
            occupancy,
            request.NumberOfGuests));

        return new QuoteDto(
            request.RoomId,
            request.CheckIn,
            request.CheckOut,
            quote.Nights,
            quote.TotalPrice,
            quote.AverageNightlyRate,
            quote.Adjustments.Select(a => $"{a.Reason}: x{a.Multiplier}").ToList());
    }
}
