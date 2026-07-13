namespace StayFlow.Application.Common.Exceptions;

/// <summary>Raised when an active reservation already holds a room for any requested night.</summary>
public sealed class ReservationConflictException : Exception
{
    public ReservationConflictException()
        : base("The room is already reserved for one or more selected nights. Choose another room or change the dates.")
    {
    }
}
