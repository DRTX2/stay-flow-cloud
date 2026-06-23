using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Application.Common.Exceptions;
using StayFlow.Domain.Rooms;
using ValidationException = StayFlow.Application.Common.Exceptions.ValidationException;

namespace StayFlow.Application.Features.Rooms.Commands;

public sealed record CreateRoomCommand(
    string Number,
    Guid RoomTypeId,
    decimal BasePrice,
    int Capacity,
    int Floor = 0) : IRequest<Guid>;

public sealed class CreateRoomValidator : AbstractValidator<CreateRoomCommand>
{
    public CreateRoomValidator()
    {
        RuleFor(x => x.Number).NotEmpty().MaximumLength(20);
        RuleFor(x => x.RoomTypeId).NotEmpty();
        RuleFor(x => x.BasePrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Capacity).GreaterThanOrEqualTo(1);
        RuleFor(x => x.Floor).GreaterThanOrEqualTo(0);
    }
}

public sealed class CreateRoomHandler(IApplicationDbContext dbContext)
    : IRequestHandler<CreateRoomCommand, Guid>
{
    public async Task<Guid> Handle(CreateRoomCommand request, CancellationToken cancellationToken)
    {
        var roomTypeExists = await dbContext.RoomTypes.AnyAsync(rt => rt.Id == request.RoomTypeId, cancellationToken);
        if (!roomTypeExists)
        {
            throw new NotFoundException(nameof(RoomType), request.RoomTypeId);
        }

        var numberTaken = await dbContext.Rooms.AnyAsync(r => r.Number == request.Number, cancellationToken);
        if (numberTaken)
        {
            throw new ValidationException(
            [
                new FluentValidation.Results.ValidationFailure(nameof(request.Number), $"Room number '{request.Number}' already exists."),
            ]);
        }

        var room = Room.Create(request.Number, request.RoomTypeId, request.BasePrice, request.Capacity, request.Floor);
        dbContext.Rooms.Add(room);
        await dbContext.SaveChangesAsync(cancellationToken);

        return room.Id;
    }
}
