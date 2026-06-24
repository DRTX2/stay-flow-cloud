using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayFlow.Application.Common.Authorization;
using StayFlow.Application.Common.Auditing;
using StayFlow.Application.Features.Audit.Queries;

namespace StayFlow.Api.Controllers;

/// <summary>Read access to the tenant's audit trail / activity stream (stored in MongoDB).</summary>
[Authorize(Policy = Permissions.AnalyticsView)]
public sealed class AuditController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AuditRecord>>> List([FromQuery] int take = 50)
        => Ok(await Sender.Send(new GetAuditTrailQuery(take)));
}
