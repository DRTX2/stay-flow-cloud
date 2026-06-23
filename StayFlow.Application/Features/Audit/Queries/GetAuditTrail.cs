using MediatR;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Application.Common.Auditing;

namespace StayFlow.Application.Features.Audit.Queries;

/// <summary>Returns the current tenant's most recent audit/activity records.</summary>
public sealed record GetAuditTrailQuery(int Take = 50) : IRequest<IReadOnlyList<AuditRecord>>;

public sealed class GetAuditTrailHandler(IAuditStore store, ITenantProvider tenantProvider)
    : IRequestHandler<GetAuditTrailQuery, IReadOnlyList<AuditRecord>>
{
    public Task<IReadOnlyList<AuditRecord>> Handle(GetAuditTrailQuery request, CancellationToken cancellationToken)
        => store.GetRecentAsync(tenantProvider.TenantId, Math.Clamp(request.Take, 1, 200), cancellationToken);
}
