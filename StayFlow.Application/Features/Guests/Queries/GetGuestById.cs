using MediatR;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Application.Common.Exceptions;
using StayFlow.Domain.Guests;

namespace StayFlow.Application.Features.Guests.Queries;

public sealed record GetGuestByIdQuery(Guid Id) : IRequest<GuestDto>;

public sealed class GetGuestByIdHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetGuestByIdQuery, GuestDto>
{
    public async Task<GuestDto> Handle(GetGuestByIdQuery request, CancellationToken cancellationToken)
    {
        var dto = await dbContext.Guests
            .AsNoTracking()
            .Where(g => g.Id == request.Id)
            .Select(g => new GuestDto(g.Id, g.FirstName, g.LastName, g.Email, g.Phone, g.DocumentNumber))
            .FirstOrDefaultAsync(cancellationToken);

        return dto ?? throw new NotFoundException(nameof(Guest), request.Id);
    }
}
