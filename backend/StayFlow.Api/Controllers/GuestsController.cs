using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayFlow.Application.Common.Authorization;
using StayFlow.Application.Common.Models;
using StayFlow.Application.Features.Guests;
using StayFlow.Application.Features.Guests.Commands;
using StayFlow.Application.Features.Guests.Queries;

namespace StayFlow.Api.Controllers;

[Authorize]
public sealed class GuestsController : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.GuestsRead)]
    public async Task<ActionResult<PagedResult<GuestDto>>> List(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
        => Ok(await Sender.Send(new GetGuestsQuery(page, pageSize, search)));

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.GuestsRead)]
    public async Task<ActionResult<GuestDto>> GetById(Guid id)
        => Ok(await Sender.Send(new GetGuestByIdQuery(id)));

    [HttpGet("{id:guid}/profile")]
    [Authorize(Policy = Permissions.GuestsRead)]
    public async Task<ActionResult<GuestProfileDto>> GetProfile(Guid id)
        => Ok(await Sender.Send(new GetGuestProfileQuery(id)));

    [HttpPost]
    [Authorize(Policy = Permissions.GuestsManage)]
    public async Task<IActionResult> Create([FromBody] CreateGuestCommand command)
    {
        var id = await Sender.Send(command);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.GuestsManage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGuestCommand command)
    {
        await Sender.Send(command with { Id = id });
        return NoContent();
    }
}
