using FluentValidation.Results;

namespace StayFlow.Application.Common.Exceptions;

/// <summary>Aggregates FluentValidation failures; surfaces as HTTP 400 with a problem document.</summary>
public sealed class ValidationException : Exception
{
    public ValidationException()
        : base("One or more validation failures have occurred.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : this()
    {
        Errors = failures
            .GroupBy(failure => failure.PropertyName, failure => failure.ErrorMessage)
            .ToDictionary(group => group.Key, group => group.ToArray());
    }

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
