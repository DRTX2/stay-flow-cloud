using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Application.Common.Exceptions;
using StayFlow.Domain.Rooms;

namespace StayFlow.Application.Features.Rooms.Commands;

public sealed record UpdateRoomPriceCommand(Guid RoomId, decimal NewPrice) : IRequest;

public sealed class UpdateRoomPriceValidator : AbstractValidator<UpdateRoomPriceCommand>
{
    public UpdateRoomPriceValidator()
    {
        RuleFor(x => x.RoomId).NotEmpty();
        RuleFor(x => x.NewPrice).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdateRoomPriceHandler(IApplicationDbContext dbContext)
    : IRequestHandler<UpdateRoomPriceCommand>
{
    public async Task Handle(UpdateRoomPriceCommand request, CancellationToken cancellationToken)
    {
        var room = await dbContext.Rooms.FirstOrDefaultAsync(r => r.Id == request.RoomId, cancellationToken)
            ?? throw new NotFoundException(nameof(Room), request.RoomId);

        room.ChangePrice(request.NewPrice);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
