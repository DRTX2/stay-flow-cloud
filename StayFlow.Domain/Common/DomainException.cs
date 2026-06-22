namespace StayFlow.Domain.Common;

/// <summary>
/// Raised when a domain invariant or business rule is violated. Surfaces as HTTP 409/422
/// at the API boundary via the global exception handler.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
