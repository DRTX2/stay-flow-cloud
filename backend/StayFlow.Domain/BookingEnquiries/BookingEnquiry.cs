using StayFlow.Domain.Common;

namespace StayFlow.Domain.BookingEnquiries;

/// <summary>A durable public booking request awaiting review and room assignment by hotel staff.</summary>
public sealed class BookingEnquiry : TenantEntity
{
    private BookingEnquiry()
    {
    }

    private BookingEnquiry(
        Guid tenantId,
        Guid roomTypeId,
        DateOnly checkIn,
        DateOnly checkOut,
        int numberOfGuests,
        string fullName,
        string email,
        string? phone)
    {
        TenantId = tenantId;
        RoomTypeId = roomTypeId;
        CheckIn = checkIn;
        CheckOut = checkOut;
        NumberOfGuests = numberOfGuests;
        FullName = fullName;
        Email = email;
        Phone = phone;
        Reference = $"SF-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
        Status = BookingEnquiryStatus.Pending;
    }

    public string Reference { get; private set; } = string.Empty;
    public Guid RoomTypeId { get; private set; }
    public DateOnly CheckIn { get; private set; }
    public DateOnly CheckOut { get; private set; }
    public int NumberOfGuests { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public BookingEnquiryStatus Status { get; private set; }
    public string? RejectionReason { get; private set; }
    public Guid? ReservationId { get; private set; }

    public static BookingEnquiry Create(
        Guid tenantId,
        Guid roomTypeId,
        DateOnly checkIn,
        DateOnly checkOut,
        int numberOfGuests,
        string fullName,
        string email,
        string? phone = null)
    {
        if (tenantId == Guid.Empty || roomTypeId == Guid.Empty)
        {
            throw new DomainException("A booking enquiry must reference a hotel and room type.");
        }

        if (checkOut <= checkIn || checkIn < DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new DomainException("Booking dates must describe a future stay.");
        }

        if (numberOfGuests is < 1 or > 20)
        {
            throw new DomainException("A booking enquiry must have between 1 and 20 guests.");
        }

        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || !email.Contains('@', StringComparison.Ordinal))
        {
            throw new DomainException("A guest name and valid email are required.");
        }

        return new BookingEnquiry(
            tenantId,
            roomTypeId,
            checkIn,
            checkOut,
            numberOfGuests,
            fullName.Trim(),
            email.Trim().ToLowerInvariant(),
            string.IsNullOrWhiteSpace(phone) ? null : phone.Trim());
    }

    public void Reject(string? reason)
    {
        EnsurePending();
        Status = BookingEnquiryStatus.Rejected;
        RejectionReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
    }

    public void MarkConverted(Guid reservationId)
    {
        EnsurePending();
        if (reservationId == Guid.Empty)
        {
            throw new DomainException("A converted enquiry must reference a reservation.");
        }

        ReservationId = reservationId;
        Status = BookingEnquiryStatus.Converted;
    }

    private void EnsurePending()
    {
        if (Status != BookingEnquiryStatus.Pending)
        {
            throw new DomainException($"Only pending enquiries can be changed (current: {Status}).");
        }
    }
}
