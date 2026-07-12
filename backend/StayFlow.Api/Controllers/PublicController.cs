using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using StayFlow.Application.Features.BookingEnquiries;
using StayFlow.Application.Features.Feedback;

namespace StayFlow.Api.Controllers;

/// <summary>
/// Anonymous, public-facing endpoints for the marketing/booking site (the Next.js public layer).
/// Kept separate from the tenant-scoped operational API: it must not require auth and must not leak
/// tenant-scoped data. Currently it accepts booking enquiries (leads) that staff confirm later.
/// </summary>
[AllowAnonymous]
[ApiController]
[Route("api/v1/public")]
[Produces("application/json")]
public sealed class PublicController(ISender sender) : ControllerBase
{
    [HttpGet("hotels")]
    public async Task<ActionResult<IReadOnlyList<PublicHotelDto>>> GetHotels()
        => Ok(await sender.Send(new GetPublicHotelsQuery()));

    [HttpPost("feedback")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> SubmitFeedback([FromBody] SubmitFeedbackCommand command)
    {
        await sender.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Accepts a public booking enquiry and returns a reference. Rate-limited (shared "auth" policy)
    /// because it is unauthenticated. This is a lead intake — it does not create operational records,
    /// which require tenant context; staff convert confirmed enquiries inside the dashboard.
    /// </summary>
    [HttpPost("bookings")]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<BookingAck>> CreateBooking([FromBody] PublicBookingRequest request)
    {
        var receipt = await sender.Send(new CreateBookingEnquiryCommand(
            request.HotelSlug,
            request.RoomTypeId,
            request.CheckIn,
            request.CheckOut,
            request.Guests,
            request.FullName,
            request.Email,
            request.Phone));
        return Accepted(new BookingAck(receipt.Reference, receipt.Status));
    }
}

/// <summary>Public booking enquiry payload. Validated by the [ApiController] model binder.</summary>
public sealed record PublicBookingRequest(
    [Required] string HotelSlug,
    [Required] Guid RoomTypeId,
    [Required] DateOnly CheckIn,
    [Required] DateOnly CheckOut,
    [Range(1, 20)] int Guests,
    [Required, StringLength(120)] string FullName,
    [Required, EmailAddress] string Email,
    string? Phone);

/// <summary>Acknowledgement returned for an accepted enquiry.</summary>
public sealed record BookingAck(string Reference, string Status);
