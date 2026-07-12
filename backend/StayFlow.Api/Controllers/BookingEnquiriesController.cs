using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayFlow.Application.Common.Authorization;
using StayFlow.Application.Common.Models;
using StayFlow.Application.Features.BookingEnquiries;
using StayFlow.Application.Features.Reservations;
using StayFlow.Domain.BookingEnquiries;

namespace StayFlow.Api.Controllers;

[Authorize]
public sealed class BookingEnquiriesController : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.ReservationsRead)]
    public async Task<ActionResult<PagedResult<BookingEnquiryDto>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] BookingEnquiryStatus? status = null)
        => Ok(await Sender.Send(new GetBookingEnquiriesQuery(page, pageSize, status)));

    [HttpPost("{id:guid}/reject")]
    [Authorize(Policy = Permissions.ReservationsManage)]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectBookingEnquiryBody? body = null)
    {
        await Sender.Send(new RejectBookingEnquiryCommand(id, body?.Reason));
        return NoContent();
    }

    [HttpPost("{id:guid}/convert")]
    [Authorize(Policy = Permissions.ReservationsManage)]
    public async Task<ActionResult<ReservationDto>> Convert(Guid id, [FromBody] ConvertBookingEnquiryBody body)
        => Ok(await Sender.Send(new ConvertBookingEnquiryCommand(id, body.RoomId)));

    public sealed record RejectBookingEnquiryBody(string? Reason);
    public sealed record ConvertBookingEnquiryBody(Guid RoomId);
}
