using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

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
public sealed class PublicController(ILogger<PublicController> logger) : ControllerBase
{
    /// <summary>
    /// Accepts a public booking enquiry and returns a reference. Rate-limited (shared "auth" policy)
    /// because it is unauthenticated. This is a lead intake — it does not create operational records,
    /// which require tenant context; staff convert confirmed enquiries inside the dashboard.
    /// </summary>
    [HttpPost("bookings")]
    [EnableRateLimiting("auth")]
    public ActionResult<BookingAck> CreateBooking([FromBody] PublicBookingRequest request)
    {
        if (request.CheckOut <= request.CheckIn)
        {
            ModelState.AddModelError(nameof(request.CheckOut), "Check-out must be after check-in.");
            return ValidationProblem(ModelState);
        }

        var reference = $"SF-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";

        logger.LogInformation(
            "Public booking enquiry {Reference}: hotel {Hotel}, room type {RoomType}, {CheckIn:yyyy-MM-dd}→{CheckOut:yyyy-MM-dd}, {Guests} guest(s), contact {Email}",
            reference, request.HotelSlug, request.RoomTypeId, request.CheckIn, request.CheckOut, request.Guests, request.Email);

        return Accepted(new BookingAck(reference, "received"));
    }
}

/// <summary>Public booking enquiry payload. Validated by the [ApiController] model binder.</summary>
public sealed record PublicBookingRequest(
    [Required] string HotelSlug,
    [Required] string RoomTypeId,
    [Required] DateOnly CheckIn,
    [Required] DateOnly CheckOut,
    [Range(1, 20)] int Guests,
    [Required, StringLength(120)] string FullName,
    [Required, EmailAddress] string Email,
    string? Phone);

/// <summary>Acknowledgement returned for an accepted enquiry.</summary>
public sealed record BookingAck(string Reference, string Status);
