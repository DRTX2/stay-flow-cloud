using MediatR;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Application.Common.Exceptions;
using StayFlow.Domain.Guests;

namespace StayFlow.Application.Features.Guests.Queries;

public sealed record GetGuestProfileQuery(Guid Id) : IRequest<GuestProfileDto>;

public sealed record GuestProfileDto(
    GuestDto Guest,
    int TotalStays,
    decimal LifetimeValue,
    DateOnly? LastStay,
    IReadOnlyList<GuestReservationHistoryDto> Reservations,
    IReadOnlyList<GuestInvoiceSummaryDto> Invoices);

public sealed record GuestReservationHistoryDto(
    Guid Id,
    Guid RoomId,
    string RoomNumber,
    DateOnly CheckIn,
    DateOnly CheckOut,
    string Status,
    decimal TotalPrice,
    string ConfirmationCode);

public sealed record GuestInvoiceSummaryDto(Guid Id, Guid ReservationId, string Number, string Status, decimal Total, DateTimeOffset? PaidAtUtc);

public sealed class GetGuestProfileHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetGuestProfileQuery, GuestProfileDto>
{
    public async Task<GuestProfileDto> Handle(GetGuestProfileQuery request, CancellationToken cancellationToken)
    {
        var guest = await dbContext.Guests
            .AsNoTracking()
            .Where(g => g.Id == request.Id)
            .Select(g => new GuestDto(g.Id, g.FirstName, g.LastName, g.Email, g.Phone, g.DocumentNumber))
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(Guest), request.Id);

        var reservations = await dbContext.Reservations
            .AsNoTracking()
            .Where(reservation => reservation.GuestId == request.Id)
            .Join(dbContext.Rooms,
                reservation => reservation.RoomId,
                room => room.Id,
                (reservation, room) => new GuestReservationHistoryDto(
                    reservation.Id,
                    reservation.RoomId,
                    room.Number,
                    reservation.Period.CheckIn,
                    reservation.Period.CheckOut,
                    reservation.Status.ToString(),
                    reservation.TotalPrice,
                    reservation.ConfirmationCode))
            .OrderByDescending(reservation => reservation.CheckIn)
            .ToListAsync(cancellationToken);

        var reservationIds = reservations.Select(reservation => reservation.Id).ToArray();
        var invoices = await dbContext.Invoices
            .AsNoTracking()
            .Where(invoice => reservationIds.Contains(invoice.ReservationId))
            .OrderByDescending(invoice => invoice.IssuedAtUtc)
            .Select(invoice => new GuestInvoiceSummaryDto(
                invoice.Id,
                invoice.ReservationId,
                invoice.Number,
                invoice.Status.ToString(),
                invoice.Total,
                invoice.PaidAtUtc))
            .ToListAsync(cancellationToken);

        var totalStays = reservations.Count(reservation => reservation.Status == "CheckedOut");
        var lastStay = reservations
            .Where(reservation => reservation.Status is "CheckedOut" or "CheckedIn")
            .OrderByDescending(reservation => reservation.CheckOut)
            .Select(reservation => (DateOnly?)reservation.CheckOut)
            .FirstOrDefault();

        return new GuestProfileDto(
            guest,
            totalStays,
            invoices.Sum(invoice => invoice.Total),
            lastStay,
            reservations,
            invoices);
    }
}
