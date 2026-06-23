using MediatR;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Application.Common.Models;
using StayFlow.Domain.Billing;

namespace StayFlow.Application.Features.Billing.Queries;

public sealed record GetInvoicesQuery(int Page = 1, int PageSize = 20, InvoiceStatus? Status = null)
    : IRequest<PagedResult<InvoiceDto>>;

public sealed class GetInvoicesHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetInvoicesQuery, PagedResult<InvoiceDto>>
{
    public async Task<PagedResult<InvoiceDto>> Handle(GetInvoicesQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = dbContext.Invoices.AsNoTracking().Include(i => i.LineItems).AsQueryable();
        if (request.Status is { } status)
        {
            query = query.Where(i => i.Status == status);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var invoices = await query
            .OrderByDescending(i => i.IssuedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = invoices.Select(InvoiceDto.FromEntity).ToList();
        return new PagedResult<InvoiceDto>(items, page, pageSize, totalCount);
    }
}
