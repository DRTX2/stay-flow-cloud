using MediatR;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;

namespace StayFlow.Application.Features.Tenants.Queries;

/// <summary>Lists all tenants. Intended for platform/super-admin use; not tenant-scoped.</summary>
public sealed record GetTenantsQuery : IRequest<IReadOnlyList<TenantDto>>;

public sealed class GetTenantsHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetTenantsQuery, IReadOnlyList<TenantDto>>
{
    public async Task<IReadOnlyList<TenantDto>> Handle(GetTenantsQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.Tenants
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .Select(t => new TenantDto(t.Id, t.Name, t.Slug, t.PropertyType, t.Plan, t.DefaultCurrency, t.IsActive))
            .ToListAsync(cancellationToken);
    }
}
