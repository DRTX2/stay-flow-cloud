using MediatR;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Application.Common.Exceptions;
using StayFlow.Domain.Reservations;

namespace StayFlow.Application.Features.Reservations.Queries;

public sealed record GetReservationByIdQuery(Guid Id) : IRequest<ReservationDto>;

public sealed class GetReservationByIdHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetReservationByIdQuery, ReservationDto>
{
    public async Task<ReservationDto> Handle(GetReservationByIdQuery request, CancellationToken cancellationToken)
    {
        var dto = await dbContext.Reservations
            .AsNoTracking()
            .Where(r => r.Id == request.Id)
            .Select(r => new ReservationDto(
                r.Id,
                r.RoomId,
                r.GuestId,
                r.Period.CheckIn,
                r.Period.CheckOut,
                r.NumberOfGuests,
                r.TotalPrice,
                r.ConfirmationCode,
                r.Status))
            .FirstOrDefaultAsync(cancellationToken);

        return dto ?? throw new NotFoundException(nameof(Reservation), request.Id);
    }
}
