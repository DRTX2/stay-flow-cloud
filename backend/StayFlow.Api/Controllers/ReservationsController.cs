using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayFlow.Application.Common.Authorization;
using StayFlow.Application.Common.Models;
using StayFlow.Application.Features.Reservations;
using StayFlow.Application.Features.Reservations.Commands;
using StayFlow.Application.Features.Reservations.Queries;
using StayFlow.Application.Features.Feedback;
using StayFlow.Domain.Reservations;

namespace StayFlow.Api.Controllers;

[Authorize]
public sealed class ReservationsController : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.ReservationsRead)]
    public async Task<ActionResult<PagedResult<ReservationDto>>> List(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] ReservationStatus? status = null)
        => Ok(await Sender.Send(new GetReservationsQuery(page, pageSize, status)));

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.ReservationsRead)]
    public async Task<ActionResult<ReservationDto>> GetById(Guid id)
        => Ok(await Sender.Send(new GetReservationByIdQuery(id)));

    [HttpGet("quote")]
    [Authorize(Policy = Permissions.ReservationsRead)]
    public async Task<ActionResult<QuoteDto>> Quote(
        [FromQuery] Guid roomId, [FromQuery] DateOnly checkIn, [FromQuery] DateOnly checkOut, [FromQuery] int numberOfGuests = 1)
        => Ok(await Sender.Send(new GetReservationQuoteQuery(roomId, checkIn, checkOut, numberOfGuests)));

    [HttpPost]
    [Authorize(Policy = Permissions.ReservationsManage)]
    public async Task<ActionResult<ReservationDto>> Create([FromBody] CreateReservationCommand command)
    {
        var dto = await Sender.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpPost("{id:guid}/confirm")]
    [Authorize(Policy = Permissions.ReservationsManage)]
    public async Task<IActionResult> Confirm(Guid id)
    {
        await Sender.Send(new ConfirmReservationCommand(id));
        return NoContent();
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = Permissions.ReservationsManage)]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelReservationBody? body = null)
    {
        await Sender.Send(new CancelReservationCommand(id, body?.Reason));
        return NoContent();
    }

    [HttpPost("{id:guid}/check-in")]
    [Authorize(Policy = Permissions.ReservationsCheckInOut)]
    public async Task<IActionResult> CheckIn(Guid id)
    {
        await Sender.Send(new CheckInReservationCommand(id));
        return NoContent();
    }

    [HttpPost("{id:guid}/check-out")]
    [Authorize(Policy = Permissions.ReservationsCheckInOut)]
    public async Task<IActionResult> CheckOut(Guid id)
    {
        await Sender.Send(new CheckOutReservationCommand(id));
        return NoContent();
    }

    [HttpPost("{id:guid}/charges")]
    [Authorize(Policy = Permissions.ReservationsManage)]
    public async Task<IActionResult> AddCharge(Guid id, [FromBody] AddChargeBody body)
    {
        var chargeId = await Sender.Send(new AddReservationChargeCommand(id, body.ServiceItemId, body.Quantity));
        return CreatedAtAction(nameof(GetById), new { id }, new { chargeId });
    }

    [HttpPost("{id:guid}/feedback-invitation")]
    [Authorize(Policy = Permissions.ReservationsManage)]
    public async Task<ActionResult<FeedbackInvitationDto>> CreateFeedbackInvitation(Guid id)
        => Ok(await Sender.Send(new CreateFeedbackInvitationCommand(id)));

    public sealed record CancelReservationBody(string? Reason);

    public sealed record AddChargeBody(Guid ServiceItemId, int Quantity = 1);
}
