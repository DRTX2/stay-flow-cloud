using MediatR;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Application.Common.Exceptions;
using StayFlow.Domain.Orders;

namespace StayFlow.Application.Features.Orders.Commands;

public sealed record MarkOrderPreparingCommand(Guid Id) : IRequest;
public sealed record MarkOrderDeliveredCommand(Guid Id) : IRequest;
public sealed record CancelOrderCommand(Guid Id) : IRequest;

internal sealed class MarkOrderPreparingHandler(IApplicationDbContext dbContext)
    : IRequestHandler<MarkOrderPreparingCommand>
{
    public async Task Handle(MarkOrderPreparingCommand request, CancellationToken cancellationToken)
    {
        var order = await FindOrder(dbContext, request.Id, cancellationToken);
        order.MarkPreparing();
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    internal static async Task<Order> FindOrder(IApplicationDbContext dbContext, Guid id, CancellationToken cancellationToken)
        => await dbContext.Orders.SingleOrDefaultAsync(order => order.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), id);
}

internal sealed class MarkOrderDeliveredHandler(IApplicationDbContext dbContext)
    : IRequestHandler<MarkOrderDeliveredCommand>
{
    public async Task Handle(MarkOrderDeliveredCommand request, CancellationToken cancellationToken)
    {
        var order = await MarkOrderPreparingHandler.FindOrder(dbContext, request.Id, cancellationToken);
        order.MarkDelivered();
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

internal sealed class CancelOrderHandler(IApplicationDbContext dbContext)
    : IRequestHandler<CancelOrderCommand>
{
    public async Task Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await MarkOrderPreparingHandler.FindOrder(dbContext, request.Id, cancellationToken);
        order.Cancel();
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
