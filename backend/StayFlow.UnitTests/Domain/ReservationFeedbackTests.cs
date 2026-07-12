using FluentAssertions;
using StayFlow.Domain.Common;
using StayFlow.Domain.Feedback;

namespace StayFlow.UnitTests.Domain;

public sealed class ReservationFeedbackTests
{
    [Fact]
    public void Submit_AcceptsOneValidResponse()
    {
        var now = DateTimeOffset.UtcNow;
        var feedback = ReservationFeedback.Create(Guid.NewGuid(), "hash", now.AddDays(30));

        feedback.Submit(5, "Excellent stay", now);

        feedback.Rating.Should().Be(5);
        feedback.Comment.Should().Be("Excellent stay");
        var secondSubmission = () => feedback.Submit(1, null, now.AddMinutes(1));
        secondSubmission.Should().Throw<DomainException>();
    }

    [Fact]
    public void Submit_RejectsExpiredInvitation()
    {
        var now = DateTimeOffset.UtcNow;
        var feedback = ReservationFeedback.Create(Guid.NewGuid(), "hash", now.AddMinutes(-1));

        var submit = () => feedback.Submit(4, null, now);

        submit.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void Submit_RejectsInvalidRating(int rating)
    {
        var now = DateTimeOffset.UtcNow;
        var feedback = ReservationFeedback.Create(Guid.NewGuid(), "hash", now.AddDays(1));

        var submit = () => feedback.Submit(rating, null, now);

        submit.Should().Throw<DomainException>();
    }
}
