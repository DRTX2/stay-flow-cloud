using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayFlow.Application.Common.Authorization;
using StayFlow.Application.Common.Models;
using StayFlow.Application.Features.Housekeeping;
using StayFlow.Application.Features.Housekeeping.Commands;
using StayFlow.Application.Features.Housekeeping.Queries;
using StayFlow.Domain.Housekeeping;
using StayFlow.Domain.Rooms;

namespace StayFlow.Api.Controllers;

[Authorize]
public sealed class HousekeepingController : ApiControllerBase
{
    [HttpGet("tasks")]
    [Authorize(Policy = Permissions.HousekeepingManage)]
    public async Task<ActionResult<PagedResult<HousekeepingTaskDto>>> ListTasks(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] HousekeepingTaskStatus? status = null)
        => Ok(await Sender.Send(new GetHousekeepingTasksQuery(page, pageSize, status)));

    [HttpPost("tasks")]
    [Authorize(Policy = Permissions.HousekeepingManage)]
    public async Task<IActionResult> CreateTask([FromBody] CreateHousekeepingTaskCommand command)
    {
        var id = await Sender.Send(command);
        return Ok(new { id });
    }

    [HttpPost("tasks/{id:guid}/complete")]
    [Authorize(Policy = Permissions.HousekeepingManage)]
    public async Task<IActionResult> CompleteTask(Guid id, [FromBody] CompleteHousekeepingTaskBody body)
    {
        await Sender.Send(new CompleteHousekeepingTaskCommand(id, body.CleaningStatus));
        return NoContent();
    }

    [HttpPut("rooms/{id:guid}/cleaning-status")]
    [Authorize(Policy = Permissions.HousekeepingManage)]
    public async Task<IActionResult> UpdateCleaningStatus(Guid id, [FromBody] UpdateCleaningStatusBody body)
    {
        await Sender.Send(new UpdateCleaningStatusCommand(id, body.Status));
        return NoContent();
    }

    public sealed record UpdateCleaningStatusBody(RoomCleaningStatus Status);

    public sealed record CompleteHousekeepingTaskBody(RoomCleaningStatus? CleaningStatus);
}
