using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Application.Common.Exceptions;
using StayFlow.Application.Features.RoomTypes.Queries;
using StayFlow.Domain.Rooms;

namespace StayFlow.Application.Features.RoomTypes.Commands;

public sealed record UpdateRoomTypeCommand(
    Guid Id,
    string Name,
    decimal BaseRate,
    int MaxOccupancy,
    string? Description = null) : IRequest;

public sealed class UpdateRoomTypeValidator : AbstractValidator<UpdateRoomTypeCommand>
{
    public UpdateRoomTypeValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.BaseRate).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxOccupancy).GreaterThanOrEqualTo(1);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}

public sealed class UpdateRoomTypeHandler(
    IApplicationDbContext dbContext,
    ICacheService cache,
    ITenantProvider tenantProvider)
    : IRequestHandler<UpdateRoomTypeCommand>
{
    public async Task Handle(UpdateRoomTypeCommand request, CancellationToken cancellationToken)
    {
        var roomType = await dbContext.RoomTypes.FirstOrDefaultAsync(rt => rt.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(RoomType), request.Id);

        roomType.Update(request.Name, request.BaseRate, request.MaxOccupancy, request.Description);
        await dbContext.SaveChangesAsync(cancellationToken);

        // Invalidate the tenant's cached room-type list.
        await cache.RemoveAsync(GetRoomTypesHandler.CacheKey(tenantProvider.TenantId), cancellationToken);
    }
}
