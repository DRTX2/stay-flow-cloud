using MediatR;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Application.Common.Exceptions;
using StayFlow.Domain.Billing;

namespace StayFlow.Application.Features.Billing.Commands;

public sealed record MarkInvoicePaidCommand(Guid Id) : IRequest;

public sealed class MarkInvoicePaidHandler(IApplicationDbContext dbContext, IDateTimeProvider clock)
    : IRequestHandler<MarkInvoicePaidCommand>
{
    public async Task Handle(MarkInvoicePaidCommand request, CancellationToken cancellationToken)
    {
        var invoice = await dbContext.Invoices.FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Invoice), request.Id);

        invoice.MarkPaid(clock.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
