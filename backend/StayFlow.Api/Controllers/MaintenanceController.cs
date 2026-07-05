using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayFlow.Application.Common.Authorization;
using StayFlow.Application.Common.Models;
using StayFlow.Application.Features.Maintenance;
using StayFlow.Application.Features.Maintenance.Commands;
using StayFlow.Application.Features.Maintenance.Queries;
using StayFlow.Domain.Maintenance;

namespace StayFlow.Api.Controllers;

[Authorize]
public sealed class MaintenanceController : ApiControllerBase
{
    [HttpGet("work-orders")]
    [Authorize(Policy = Permissions.MaintenanceManage)]
    public async Task<ActionResult<PagedResult<WorkOrderDto>>> ListWorkOrders(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] WorkOrderStatus? status = null)
        => Ok(await Sender.Send(new GetWorkOrdersQuery(page, pageSize, status)));

    [HttpPost("work-orders")]
    [Authorize(Policy = Permissions.MaintenanceManage)]
    public async Task<IActionResult> CreateWorkOrder([FromBody] CreateWorkOrderCommand command)
    {
        var id = await Sender.Send(command);
        return Ok(new { id });
    }

    [HttpPost("work-orders/{id:guid}/resolve")]
    [Authorize(Policy = Permissions.MaintenanceManage)]
    public async Task<IActionResult> ResolveWorkOrder(Guid id, [FromBody] ResolveWorkOrderBody body)
    {
        await Sender.Send(new ResolveWorkOrderCommand(id, body.Notes));
        return NoContent();
    }

    public sealed record ResolveWorkOrderBody(string? Notes);
}
