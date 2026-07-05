using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayFlow.Application.Common.Authorization;
using StayFlow.Application.Features.Demo;

namespace StayFlow.Api.Controllers;

[Authorize]
public sealed class DemoController : ApiControllerBase
{
    [HttpPost("sample-stay")]
    [Authorize(Policy = Permissions.ReservationsManage)]
    public async Task<ActionResult<SampleStayDto>> RunSampleStay()
        => Ok(await Sender.Send(new RunSampleStayCommand()));
}
