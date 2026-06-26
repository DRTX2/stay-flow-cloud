using StayFlow.Domain.Common;

namespace StayFlow.Domain.Guests;

/// <summary>A guest profile owned by a tenant. Reused across reservations.</summary>
public sealed class Guest : TenantEntity
{
    private Guest()
    {
    }

    private Guest(string firstName, string lastName, string email, string? phone, string? documentNumber)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Phone = phone;
        DocumentNumber = documentNumber;
    }

    public string FirstName { get; private set; } = string.Empty;

    public string LastName { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string? Phone { get; private set; }

    public string? DocumentNumber { get; private set; }

    public string FullName => $"{FirstName} {LastName}";

    public static Guest Create(string firstName, string lastName, string email, string? phone = null, string? documentNumber = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new DomainException("Guest first name is required.");
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new DomainException("Guest last name is required.");
        }

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@', StringComparison.Ordinal))
        {
            throw new DomainException("A valid guest email is required.");
        }

        return new Guest(firstName.Trim(), lastName.Trim(), email.Trim().ToLowerInvariant(), phone?.Trim(), documentNumber?.Trim());
    }

    public void UpdateContact(string email, string? phone)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@', StringComparison.Ordinal))
        {
            throw new DomainException("A valid guest email is required.");
        }

        Email = email.Trim().ToLowerInvariant();
        Phone = phone?.Trim();
    }

    public void UpdateProfile(string firstName, string lastName, string email, string? phone, string? documentNumber)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new DomainException("Guest first name is required.");
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new DomainException("Guest last name is required.");
        }

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@', StringComparison.Ordinal))
        {
            throw new DomainException("A valid guest email is required.");
        }

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = email.Trim().ToLowerInvariant();
        Phone = phone?.Trim();
        DocumentNumber = documentNumber?.Trim();
    }
}
