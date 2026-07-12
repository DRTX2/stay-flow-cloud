using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayFlow.Application.Common.Authorization;
using StayFlow.Application.Common.Models;
using StayFlow.Application.Features.Feedback;

namespace StayFlow.Api.Controllers;

[Authorize]
public sealed class FeedbackController : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.FeedbackRead)]
    public async Task<ActionResult<PagedResult<FeedbackDto>>> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(await Sender.Send(new GetFeedbackQuery(page, pageSize)));
}
