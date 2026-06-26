using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayFlow.Application.Common.Authorization;
using StayFlow.Application.Features.RoomTypes;
using StayFlow.Application.Features.RoomTypes.Commands;
using StayFlow.Application.Features.RoomTypes.Queries;

namespace StayFlow.Api.Controllers;

[Authorize]
public sealed class RoomTypesController : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.RoomsRead)]
    public async Task<ActionResult<IReadOnlyList<RoomTypeDto>>> List()
        => Ok(await Sender.Send(new GetRoomTypesQuery()));

    [HttpPost]
    [Authorize(Policy = Permissions.RoomsManage)]
    public async Task<IActionResult> Create([FromBody] CreateRoomTypeCommand command)
    {
        var id = await Sender.Send(command);
        return CreatedAtAction(nameof(List), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.RoomsManage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoomTypeCommand command)
    {
        await Sender.Send(command with { Id = id });
        return NoContent();
    }
}
