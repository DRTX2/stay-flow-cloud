namespace StayFlow.Application.Common.Abstractions;

/// <summary>Abstracts the system clock so time-dependent logic stays deterministic in tests.</summary>
public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}
