using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayFlow.Application.Common.Authorization;
using StayFlow.Application.Features.Tenants.Commands;
using StayFlow.Application.Features.Tenants.Queries;
using StayFlow.Domain.Tenants;

namespace StayFlow.Api.Controllers;

/// <summary>The current tenant's plan, limits and feature flags. Routed at /api/v1/tenantfeatures.</summary>
[Authorize]
public sealed class TenantFeaturesController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<TenantFeaturesDto>> Get()
        => Ok(await Sender.Send(new GetTenantFeaturesQuery()));

    [HttpPut("{feature}")]
    [Authorize(Policy = Permissions.FeaturesManage)]
    public async Task<IActionResult> Set(Feature feature, [FromBody] SetFeatureBody body)
    {
        await Sender.Send(new SetTenantFeatureCommand(feature, body.Enabled));
        return NoContent();
    }

    public sealed record SetFeatureBody(bool Enabled);
}
