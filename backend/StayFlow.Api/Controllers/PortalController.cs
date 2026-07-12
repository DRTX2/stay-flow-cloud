using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayFlow.Application.Features.Guests;
using StayFlow.Application.Features.Portal;
using StayFlow.Application.Features.Reservations;

namespace StayFlow.Api.Controllers;

[Authorize]
public sealed class PortalController : ApiControllerBase
{
    [HttpGet("reservations")]
    public async Task<ActionResult<IReadOnlyList<ReservationDto>>> Reservations()
        => Ok(await Sender.Send(new GetMyReservationsQuery()));

    [HttpGet("profile")]
    public async Task<ActionResult<GuestDto>> Profile()
        => Ok(await Sender.Send(new GetMyProfileQuery()));

    [HttpPut("profile")]
    public async Task<ActionResult<GuestDto>> UpdateProfile([FromBody] UpdateMyProfileCommand command)
        => Ok(await Sender.Send(command));
}
