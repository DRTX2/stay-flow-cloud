using FluentValidation;
using MediatR;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Domain.Rooms;

namespace StayFlow.Application.Features.RoomTypes.Commands;

public sealed record CreateRoomTypeCommand(
    string Name,
    decimal BaseRate,
    int MaxOccupancy,
    string? Description = null) : IRequest<Guid>;

public sealed class CreateRoomTypeValidator : AbstractValidator<CreateRoomTypeCommand>
{
    public CreateRoomTypeValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.BaseRate).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxOccupancy).GreaterThanOrEqualTo(1);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}

public sealed class CreateRoomTypeHandler(IApplicationDbContext dbContext)
    : IRequestHandler<CreateRoomTypeCommand, Guid>
{
    public async Task<Guid> Handle(CreateRoomTypeCommand request, CancellationToken cancellationToken)
    {
        var roomType = RoomType.Create(request.Name, request.BaseRate, request.MaxOccupancy, request.Description);
        dbContext.RoomTypes.Add(roomType);
        await dbContext.SaveChangesAsync(cancellationToken);

        return roomType.Id;
    }
}
