using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayFlow.Application.Common.Authorization;
using StayFlow.Application.Features.Staff.Commands;
using StayFlow.Application.Features.Staff.Queries;

namespace StayFlow.Api.Controllers;

[Authorize(Policy = Permissions.StaffManage)]
public sealed class StaffController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<StaffUsersDto>> List()
        => Ok(await Sender.Send(new GetStaffUsersQuery()));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStaffUserCommand command)
    {
        var id = await Sender.Send(command);
        return CreatedAtAction(nameof(List), new { id }, new { id });
    }

    [HttpPut("{id:guid}/roles")]
    public async Task<IActionResult> UpdateRoles(Guid id, [FromBody] UpdateStaffRolesBody body)
    {
        await Sender.Send(new UpdateStaffRolesCommand(id, body.Roles));
        return NoContent();
    }

    public sealed record UpdateStaffRolesBody(IReadOnlyList<string> Roles);
}
