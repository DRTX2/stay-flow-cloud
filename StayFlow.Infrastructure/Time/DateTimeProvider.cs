using StayFlow.Application.Common.Abstractions;

namespace StayFlow.Infrastructure.Time;

/// <summary>System-clock implementation of <see cref="IDateTimeProvider"/>.</summary>
public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
