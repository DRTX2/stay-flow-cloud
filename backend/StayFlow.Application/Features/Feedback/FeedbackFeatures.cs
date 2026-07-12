using System.Security.Cryptography;
using System.Text;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Application.Common.Exceptions;
using StayFlow.Application.Common.Models;
using StayFlow.Domain.Feedback;
using StayFlow.Domain.Reservations;

namespace StayFlow.Application.Features.Feedback;

public sealed record FeedbackInvitationDto(string Token, DateTimeOffset ExpiresAtUtc);
public sealed record CreateFeedbackInvitationCommand(Guid ReservationId) : IRequest<FeedbackInvitationDto>;

public sealed class CreateFeedbackInvitationHandler(IApplicationDbContext dbContext, IDateTimeProvider clock)
    : IRequestHandler<CreateFeedbackInvitationCommand, FeedbackInvitationDto>
{
    public async Task<FeedbackInvitationDto> Handle(CreateFeedbackInvitationCommand request, CancellationToken cancellationToken)
    {
        var reservation = await dbContext.Reservations.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == request.ReservationId, cancellationToken)
            ?? throw new NotFoundException(nameof(Reservation), request.ReservationId);
        if (reservation.Status != ReservationStatus.CheckedOut)
        {
            throw new Domain.Common.DomainException("Feedback invitations are available only after checkout.");
        }

        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
        var hash = FeedbackToken.Hash(token);
        var expiresAtUtc = clock.UtcNow.AddDays(30);
        var feedback = await dbContext.ReservationFeedback
            .SingleOrDefaultAsync(item => item.ReservationId == request.ReservationId, cancellationToken);
        if (feedback is null)
        {
            feedback = ReservationFeedback.Create(request.ReservationId, hash, expiresAtUtc);
            dbContext.ReservationFeedback.Add(feedback);
        }
        else
        {
            feedback.RotateInvitation(hash, expiresAtUtc);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return new FeedbackInvitationDto(token, expiresAtUtc);
    }
}

public sealed record SubmitFeedbackCommand(string Token, int Rating, string? Comment) : IRequest;

public sealed class SubmitFeedbackValidator : AbstractValidator<SubmitFeedbackCommand>
{
    public SubmitFeedbackValidator()
    {
        RuleFor(x => x.Token).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Rating).InclusiveBetween(1, 5);
        RuleFor(x => x.Comment).MaximumLength(2000);
    }
}

public sealed class SubmitFeedbackHandler(IApplicationDbContext dbContext, IDateTimeProvider clock)
    : IRequestHandler<SubmitFeedbackCommand>
{
    public async Task Handle(SubmitFeedbackCommand request, CancellationToken cancellationToken)
    {
        var hash = FeedbackToken.Hash(request.Token);
        var feedback = await dbContext.ReservationFeedback.IgnoreQueryFilters()
            .SingleOrDefaultAsync(item => item.InvitationTokenHash == hash && !item.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("Feedback invitation", "token");
        var validStay = await dbContext.Reservations.IgnoreQueryFilters().AsNoTracking()
            .AnyAsync(reservation => reservation.Id == feedback.ReservationId
                && reservation.TenantId == feedback.TenantId
                && !reservation.IsDeleted
                && reservation.Status == ReservationStatus.CheckedOut, cancellationToken);
        if (!validStay)
        {
            throw new Domain.Common.DomainException("The associated stay is not eligible for feedback.");
        }

        feedback.Submit(request.Rating, request.Comment, clock.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

public sealed record FeedbackDto(
    Guid Id,
    Guid ReservationId,
    string ConfirmationCode,
    string GuestName,
    int Rating,
    string? Comment,
    DateTimeOffset SubmittedAtUtc);

public sealed record GetFeedbackQuery(int Page = 1, int PageSize = 20) : IRequest<PagedResult<FeedbackDto>>;

public sealed class GetFeedbackHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetFeedbackQuery, PagedResult<FeedbackDto>>
{
    public async Task<PagedResult<FeedbackDto>> Handle(GetFeedbackQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var query = dbContext.ReservationFeedback.AsNoTracking().Where(feedback => feedback.SubmittedAtUtc != null);
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Join(dbContext.Reservations, feedback => feedback.ReservationId, reservation => reservation.Id, (feedback, reservation) => new { feedback, reservation })
            .Join(dbContext.Guests, item => item.reservation.GuestId, guest => guest.Id, (item, guest) => new { item.feedback, item.reservation, guest })
            .OrderByDescending(item => item.feedback.SubmittedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(item => new FeedbackDto(
                item.feedback.Id,
                item.reservation.Id,
                item.reservation.ConfirmationCode,
                item.guest.FirstName + " " + item.guest.LastName,
                item.feedback.Rating!.Value,
                item.feedback.Comment,
                item.feedback.SubmittedAtUtc!.Value))
            .ToListAsync(cancellationToken);
        return new PagedResult<FeedbackDto>(items, page, pageSize, total);
    }
}

internal static class FeedbackToken
{
    public static string Hash(string token)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
