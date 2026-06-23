using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayFlow.Application.Common.Authorization;
using StayFlow.Application.Common.Models;
using StayFlow.Application.Features.Rooms;
using StayFlow.Application.Features.Rooms.Commands;
using StayFlow.Application.Features.Rooms.Queries;
using StayFlow.Domain.Rooms;

namespace StayFlow.Api.Controllers;

[Authorize]
public sealed class RoomsController : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.RoomsRead)]
    public async Task<ActionResult<PagedResult<RoomDto>>> List(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] RoomStatus? status = null)
        => Ok(await Sender.Send(new GetRoomsQuery(page, pageSize, status)));

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.RoomsRead)]
    public async Task<ActionResult<RoomDto>> GetById(Guid id)
        => Ok(await Sender.Send(new GetRoomByIdQuery(id)));

    [HttpPost]
    [Authorize(Policy = Permissions.RoomsManage)]
    public async Task<IActionResult> Create([FromBody] CreateRoomCommand command)
    {
        var id = await Sender.Send(command);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:guid}/price")]
    [Authorize(Policy = Permissions.RoomsManage)]
    public async Task<IActionResult> UpdatePrice(Guid id, [FromBody] UpdateRoomPriceBody body)
    {
        await Sender.Send(new UpdateRoomPriceCommand(id, body.NewPrice));
        return NoContent();
    }

    public sealed record UpdateRoomPriceBody(decimal NewPrice);
}
