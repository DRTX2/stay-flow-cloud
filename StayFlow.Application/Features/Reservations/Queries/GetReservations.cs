using MediatR;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Application.Common.Models;
using StayFlow.Domain.Reservations;

namespace StayFlow.Application.Features.Reservations.Queries;

public sealed record GetReservationsQuery(int Page = 1, int PageSize = 20, ReservationStatus? Status = null)
    : IRequest<PagedResult<ReservationDto>>;

public sealed class GetReservationsHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetReservationsQuery, PagedResult<ReservationDto>>
{
    public async Task<PagedResult<ReservationDto>> Handle(GetReservationsQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = dbContext.Reservations.AsNoTracking();
        if (request.Status is { } status)
        {
            query = query.Where(r => r.Status == status);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(r => r.Period.CheckIn)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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
            .ToListAsync(cancellationToken);

        return new PagedResult<ReservationDto>(items, page, pageSize, totalCount);
    }
}
