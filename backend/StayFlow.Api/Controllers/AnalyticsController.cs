using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayFlow.Application.Common.Authorization;
using StayFlow.Application.Features.Analytics.Queries;

namespace StayFlow.Api.Controllers;

/// <summary>Operational reporting for the current tenant (dashboards, revenue trend).</summary>
[Authorize(Policy = Permissions.AnalyticsView)]
public sealed class AnalyticsController : ApiControllerBase
{
    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardSummaryDto>> Dashboard()
        => Ok(await Sender.Send(new GetDashboardSummaryQuery()));

    [HttpGet("revenue")]
    public async Task<ActionResult<RevenueReportDto>> Revenue([FromQuery] int days = 30)
        => Ok(await Sender.Send(new GetRevenueReportQuery(days)));
}
