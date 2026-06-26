using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayFlow.Application.Common.Authorization;
using StayFlow.Application.Features.Services;
using StayFlow.Application.Features.Services.Commands;
using StayFlow.Application.Features.Services.Queries;

namespace StayFlow.Api.Controllers;

[Authorize]
public sealed class ServicesController : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.ServicesRead)]
    public async Task<ActionResult<IReadOnlyList<ServiceItemDto>>> List([FromQuery] bool activeOnly = false)
        => Ok(await Sender.Send(new GetServiceItemsQuery(activeOnly)));

    [HttpPost]
    [Authorize(Policy = Permissions.ServicesManage)]
    public async Task<IActionResult> Create([FromBody] CreateServiceItemCommand command)
    {
        var id = await Sender.Send(command);
        return CreatedAtAction(nameof(List), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.ServicesManage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateServiceItemCommand command)
    {
        await Sender.Send(command with { Id = id });
        return NoContent();
    }
}
