using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Domain.Reservations;

namespace StayFlow.Application.Features.Analytics.Queries;

/// <summary>Daily booked-revenue trend (from checked-out reservations) for the current tenant.</summary>
public sealed record GetRevenueReportQuery(int Days = 30) : IRequest<RevenueReportDto>;

public sealed record RevenuePoint(DateOnly Date, decimal Revenue, int Checkouts);

public sealed record RevenueReportDto(
    DateOnly From,
    DateOnly To,
    decimal Total,
    IReadOnlyList<RevenuePoint> Daily);

public sealed class GetRevenueReportValidator : AbstractValidator<GetRevenueReportQuery>
{
    public GetRevenueReportValidator()
    {
        RuleFor(x => x.Days).InclusiveBetween(1, 365);
    }
}

public sealed class GetRevenueReportHandler(IApplicationDbContext dbContext, IDateTimeProvider clock)
    : IRequestHandler<GetRevenueReportQuery, RevenueReportDto>
{
    public async Task<RevenueReportDto> Handle(GetRevenueReportQuery request, CancellationToken cancellationToken)
    {
        var to = DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);
        var from = to.AddDays(-(request.Days - 1));

        // Filter + project columns in SQL; group in memory, since EF can't translate GroupBy over
        // an owned-type property (Period.CheckOut).
        var checkouts = await dbContext.Reservations
            .Where(r => r.Status == ReservationStatus.CheckedOut
                && r.Period.CheckOut >= from
                && r.Period.CheckOut <= to)
            .Select(r => new { Date = r.Period.CheckOut, r.TotalPrice })
            .ToListAsync(cancellationToken);

        var daily = checkouts
            .GroupBy(x => x.Date)
            .Select(g => new RevenuePoint(g.Key, g.Sum(x => x.TotalPrice), g.Count()))
            .OrderBy(p => p.Date)
            .ToList();

        return new RevenueReportDto(from, to, daily.Sum(p => p.Revenue), daily);
    }
}
