using MediatR;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Application.Common.Exceptions;
using StayFlow.Application.Features.Guests;
using StayFlow.Application.Features.Reservations;

namespace StayFlow.Application.Features.Portal;

public sealed record GetMyReservationsQuery : IRequest<IReadOnlyList<ReservationDto>>;

public sealed class GetMyReservationsHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
    : IRequestHandler<GetMyReservationsQuery, IReadOnlyList<ReservationDto>>
{
    public async Task<IReadOnlyList<ReservationDto>> Handle(GetMyReservationsQuery request, CancellationToken cancellationToken)
    {
        var guestId = RequireGuestId(currentUser);
        return await dbContext.Reservations.AsNoTracking()
            .Where(reservation => reservation.GuestId == guestId)
            .OrderByDescending(reservation => reservation.Period.CheckIn)
            .Select(reservation => new ReservationDto(
                reservation.Id,
                reservation.RoomId,
                reservation.GuestId,
                reservation.Period.CheckIn,
                reservation.Period.CheckOut,
                reservation.NumberOfGuests,
                reservation.TotalPrice,
                reservation.ConfirmationCode,
                reservation.Status))
            .ToListAsync(cancellationToken);
    }

    internal static Guid RequireGuestId(ICurrentUser currentUser)
    {
        if (!currentUser.IsAuthenticated || currentUser.GuestId is not { } guestId)
        {
            throw new Domain.Common.DomainException("This account has not claimed a guest profile.");
        }
        return guestId;
    }
}

public sealed record GetMyProfileQuery : IRequest<GuestDto>;

public sealed class GetMyProfileHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
    : IRequestHandler<GetMyProfileQuery, GuestDto>
{
    public async Task<GuestDto> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        var guestId = GetMyReservationsHandler.RequireGuestId(currentUser);
        var guest = await dbContext.Guests.AsNoTracking().SingleOrDefaultAsync(item => item.Id == guestId, cancellationToken)
            ?? throw new NotFoundException("Guest profile", guestId);
        return new GuestDto(guest.Id, guest.FirstName, guest.LastName, guest.Email, guest.Phone, guest.DocumentNumber);
    }
}

public sealed record UpdateMyProfileCommand(string FirstName, string LastName, string? Phone, string? DocumentNumber) : IRequest<GuestDto>;

public sealed class UpdateMyProfileHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
    : IRequestHandler<UpdateMyProfileCommand, GuestDto>
{
    public async Task<GuestDto> Handle(UpdateMyProfileCommand request, CancellationToken cancellationToken)
    {
        var guestId = GetMyReservationsHandler.RequireGuestId(currentUser);
        var guest = await dbContext.Guests.SingleOrDefaultAsync(item => item.Id == guestId, cancellationToken)
            ?? throw new NotFoundException("Guest profile", guestId);
        guest.UpdateProfile(request.FirstName, request.LastName, guest.Email, request.Phone, request.DocumentNumber);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new GuestDto(guest.Id, guest.FirstName, guest.LastName, guest.Email, guest.Phone, guest.DocumentNumber);
    }
}
