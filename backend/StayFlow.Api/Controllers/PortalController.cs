using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayFlow.Application.Features.Guests;
using StayFlow.Application.Features.Portal;
using StayFlow.Application.Features.Reservations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using StayFlow.Application.Common.Authorization;
using StayFlow.Persistence;
using StayFlow.Persistence.Identity;

namespace StayFlow.Api.Controllers;

[Authorize(Roles = Roles.Customer)]
public sealed class PortalController(
    StayFlowDbContext dbContext,
    UserManager<ApplicationUser> userManager) : ApiControllerBase
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

    [HttpPost("link")]
    public async Task<IActionResult> LinkGuest([FromBody] LinkGuestRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Token) || request.Token.Length > 256)
        {
            return BadRequest("A valid invitation token is required.");
        }

        var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(request.Token.Trim())));
        var strategy = dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            var user = await userManager.GetUserAsync(User);
            if (user is null)
            {
                return (IActionResult)Unauthorized();
            }

            var invitation = await dbContext.PortalGuestInvitations
                .SingleOrDefaultAsync(item => item.TokenHash == tokenHash, cancellationToken);
            if (invitation is null || invitation.RedeemedAtUtc is not null || invitation.ExpiresAtUtc <= DateTimeOffset.UtcNow)
            {
                return BadRequest("The invitation is invalid, expired, or has already been used.");
            }
            if (user.GuestId is not null && (user.GuestId != invitation.GuestId || user.TenantId != invitation.TenantId))
            {
                return Conflict("This account is already linked to another guest profile.");
            }
            if (!await dbContext.Guests.IgnoreQueryFilters().AnyAsync(
                    guest => guest.Id == invitation.GuestId
                        && guest.TenantId == invitation.TenantId
                        && !guest.IsDeleted,
                    cancellationToken))
            {
                return BadRequest("The invited guest profile is no longer available.");
            }
            if (await dbContext.Users.AnyAsync(
                    other => other.Id != user.Id && other.TenantId == invitation.TenantId && other.GuestId == invitation.GuestId,
                    cancellationToken))
            {
                return Conflict("This guest profile has already been claimed.");
            }

            var redeemedAt = DateTimeOffset.UtcNow;
            var updated = await dbContext.PortalGuestInvitations
                .Where(item => item.Id == invitation.Id && item.RedeemedAtUtc == null && item.ExpiresAtUtc > redeemedAt)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(item => item.RedeemedAtUtc, redeemedAt)
                    .SetProperty(item => item.RedeemedByUserId, user.Id), cancellationToken);
            if (updated != 1)
            {
                return Conflict("The invitation has already been used.");
            }

            user.TenantId = invitation.TenantId;
            user.GuestId = invitation.GuestId;
            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return Problem("The guest profile could not be linked.");
            }

            await transaction.CommitAsync(cancellationToken);
            return NoContent();
        });
    }

    public sealed record LinkGuestRequest(string Token);
}
