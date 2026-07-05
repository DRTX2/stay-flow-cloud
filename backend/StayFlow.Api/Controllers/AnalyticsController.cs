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

    [HttpGet("front-desk/today")]
    public async Task<ActionResult<FrontDeskTodayDto>> FrontDeskToday([FromQuery] DateOnly? date = null)
        => Ok(await Sender.Send(new GetFrontDeskTodayQuery(date)));

    [HttpGet("room-rack")]
    public async Task<ActionResult<RoomRackDto>> RoomRack([FromQuery] DateOnly? from = null, [FromQuery] DateOnly? to = null)
        => Ok(await Sender.Send(new GetRoomRackQuery(from, to)));

    [HttpGet("setup-checklist")]
    public async Task<ActionResult<SetupChecklistDto>> SetupChecklist()
        => Ok(await Sender.Send(new GetSetupChecklistQuery()));
}
