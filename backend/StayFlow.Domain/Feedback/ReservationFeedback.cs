using StayFlow.Domain.Common;

namespace StayFlow.Domain.Feedback;

/// <summary>Private post-stay feedback tied to a one-time, expiring invitation.</summary>
public sealed class ReservationFeedback : TenantEntity
{
    private ReservationFeedback()
    {
    }

    private ReservationFeedback(Guid reservationId, string tokenHash, DateTimeOffset expiresAtUtc)
    {
        ReservationId = reservationId;
        InvitationTokenHash = tokenHash;
        InvitationExpiresAtUtc = expiresAtUtc;
    }

    public Guid ReservationId { get; private set; }
    public string InvitationTokenHash { get; private set; } = string.Empty;
    public DateTimeOffset InvitationExpiresAtUtc { get; private set; }
    public int? Rating { get; private set; }
    public string? Comment { get; private set; }
    public DateTimeOffset? SubmittedAtUtc { get; private set; }

    public static ReservationFeedback Create(Guid reservationId, string tokenHash, DateTimeOffset expiresAtUtc)
    {
        if (reservationId == Guid.Empty || string.IsNullOrWhiteSpace(tokenHash))
        {
            throw new DomainException("A feedback invitation requires a reservation and token.");
        }

        return new ReservationFeedback(reservationId, tokenHash, expiresAtUtc);
    }

    public void RotateInvitation(string tokenHash, DateTimeOffset expiresAtUtc)
    {
        if (SubmittedAtUtc is not null)
        {
            throw new DomainException("Feedback has already been submitted for this stay.");
        }

        InvitationTokenHash = tokenHash;
        InvitationExpiresAtUtc = expiresAtUtc;
    }

    public void Submit(int rating, string? comment, DateTimeOffset submittedAtUtc)
    {
        if (SubmittedAtUtc is not null)
        {
            throw new DomainException("This feedback invitation has already been used.");
        }

        if (submittedAtUtc > InvitationExpiresAtUtc)
        {
            throw new DomainException("This feedback invitation has expired.");
        }

        if (rating is < 1 or > 5)
        {
            throw new DomainException("Rating must be between 1 and 5.");
        }

        Rating = rating;
        Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        SubmittedAtUtc = submittedAtUtc;
    }
}
