namespace StayFlow.Application.Common.Exceptions;

/// <summary>Raised when a requested resource does not exist (or is invisible to the tenant). Maps to HTTP 404.</summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string name, object key)
        : base($"{name} with key '{key}' was not found.")
    {
    }
}
