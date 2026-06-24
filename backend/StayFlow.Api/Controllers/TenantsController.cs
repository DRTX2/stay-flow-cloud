using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayFlow.Application.Common.Authorization;
using StayFlow.Application.Features.Tenants;
using StayFlow.Application.Features.Tenants.Commands;
using StayFlow.Application.Features.Tenants.Queries;

namespace StayFlow.Api.Controllers;

/// <summary>Platform-level tenant administration. Restricted to the super-admin (tenants:manage).</summary>
[Authorize(Policy = Permissions.TenantsManage)]
public sealed class TenantsController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TenantDto>>> List()
        => Ok(await Sender.Send(new GetTenantsQuery()));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTenantCommand command)
    {
        var id = await Sender.Send(command);
        return CreatedAtAction(nameof(List), new { id }, new { id });
    }
}
