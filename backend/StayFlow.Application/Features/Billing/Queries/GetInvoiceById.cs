using MediatR;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Application.Common.Exceptions;
using StayFlow.Domain.Billing;

namespace StayFlow.Application.Features.Billing.Queries;

public sealed record GetInvoiceByIdQuery(Guid Id) : IRequest<InvoiceDto>;

public sealed class GetInvoiceByIdHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetInvoiceByIdQuery, InvoiceDto>
{
    public async Task<InvoiceDto> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
        var invoice = await dbContext.Invoices
            .AsNoTracking()
            .Include(i => i.LineItems)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Invoice), request.Id);

        return InvoiceDto.FromEntity(invoice);
    }
}
