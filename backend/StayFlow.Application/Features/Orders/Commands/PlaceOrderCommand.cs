using MediatR;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Application.Common.Exceptions;
using StayFlow.Domain.Common;
using StayFlow.Domain.Orders;
using StayFlow.Domain.Reservations;
using StayFlow.Domain.Services;

namespace StayFlow.Application.Features.Orders.Commands;

public sealed record OrderLineItemInput(Guid ServiceItemId, int Quantity);

public sealed record PlaceOrderCommand(Guid ReservationId, string? Notes, List<OrderLineItemInput> Items) : IRequest<Guid>;

internal sealed class PlaceOrderCommandHandler(IApplicationDbContext dbContext) : IRequestHandler<PlaceOrderCommand, Guid>
{
    public async Task<Guid> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        if (request.Items.Count == 0)
        {
            throw new DomainException("Order must contain at least one item.");
        }

        var reservationIsCheckedIn = await dbContext.Reservations
            .AnyAsync(reservation => reservation.Id == request.ReservationId
                && reservation.Status == ReservationStatus.CheckedIn, cancellationToken);
        if (!reservationIsCheckedIn)
        {
            throw new DomainException("Orders can only be placed for checked-in reservations.");
        }

        var order = Order.Create(request.ReservationId, request.Notes);

        var serviceItemIds = request.Items.Select(i => i.ServiceItemId).ToList();
        var services = await dbContext.ServiceItems
            .Where(s => serviceItemIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, cancellationToken);

        foreach (var itemInput in request.Items)
        {
            if (!services.TryGetValue(itemInput.ServiceItemId, out var service))
            {
                throw new DomainException($"Service item {itemInput.ServiceItemId} not found.");
            }
            
            order.AddItem(service.Id, service.Name, itemInput.Quantity, service.Price);
        }
        
        order.Place();
        
        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        return order.Id;
    }
}
