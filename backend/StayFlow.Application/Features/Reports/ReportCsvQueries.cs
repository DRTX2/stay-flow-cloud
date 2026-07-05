using System.Globalization;
using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Domain.Housekeeping;
using StayFlow.Domain.Maintenance;
using StayFlow.Domain.Reservations;
using StayFlow.Domain.Rooms;

namespace StayFlow.Application.Features.Reports;

public sealed record CsvReportDto(string FileName, string Content, string ContentType = "text/csv; charset=utf-8");

public sealed record GetOccupancyCsvReportQuery(DateOnly? From = null, DateOnly? To = null) : IRequest<CsvReportDto>;

public sealed record GetRevenueCsvReportQuery(int Days = 30) : IRequest<CsvReportDto>;

public sealed record GetArrivalsDeparturesCsvReportQuery(DateOnly? Date = null) : IRequest<CsvReportDto>;

public sealed record GetNightAuditCsvReportQuery(DateOnly? Date = null) : IRequest<CsvReportDto>;

public sealed class GetOccupancyCsvReportHandler(IApplicationDbContext dbContext, IDateTimeProvider clock)
    : IRequestHandler<GetOccupancyCsvReportQuery, CsvReportDto>
{
    private static readonly ReservationStatus[] ActiveStatuses =
    [
        ReservationStatus.Pending,
        ReservationStatus.Confirmed,
        ReservationStatus.CheckedIn,
    ];

    public async Task<CsvReportDto> Handle(GetOccupancyCsvReportQuery request, CancellationToken cancellationToken)
    {
        var to = request.To ?? DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);
        var from = request.From ?? to.AddDays(-29);
        if (to < from)
        {
            (from, to) = (to, from);
        }

        var totalRooms = await dbContext.Rooms.CountAsync(cancellationToken);
        var reservations = await dbContext.Reservations
            .AsNoTracking()
            .Where(reservation => ActiveStatuses.Contains(reservation.Status)
                && reservation.Period.CheckIn <= to
                && reservation.Period.CheckOut > from)
            .Select(reservation => new { reservation.RoomId, reservation.Period.CheckIn, reservation.Period.CheckOut })
            .ToListAsync(cancellationToken);

        var csv = new CsvBuilder("Date", "Total Rooms", "Occupied Rooms", "Occupancy Rate");
        for (var date = from; date <= to; date = date.AddDays(1))
        {
            var occupiedRooms = reservations
                .Where(reservation => reservation.CheckIn <= date && date < reservation.CheckOut)
                .Select(reservation => reservation.RoomId)
                .Distinct()
                .Count();
            var occupancyRate = totalRooms == 0 ? 0m : Math.Round(occupiedRooms * 100m / totalRooms, 2);
            csv.Add(date, totalRooms, occupiedRooms, occupancyRate);
        }

        return new CsvReportDto($"stayflow-occupancy-{from:yyyyMMdd}-{to:yyyyMMdd}.csv", csv.ToString());
    }
}

public sealed class GetRevenueCsvReportHandler(IApplicationDbContext dbContext, IDateTimeProvider clock)
    : IRequestHandler<GetRevenueCsvReportQuery, CsvReportDto>
{
    public async Task<CsvReportDto> Handle(GetRevenueCsvReportQuery request, CancellationToken cancellationToken)
    {
        var days = Math.Clamp(request.Days, 1, 365);
        var to = DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);
        var from = to.AddDays(-(days - 1));

        var checkouts = await dbContext.Reservations
            .AsNoTracking()
            .Where(reservation => reservation.Status == ReservationStatus.CheckedOut
                && reservation.Period.CheckOut >= from
                && reservation.Period.CheckOut <= to)
            .Select(reservation => new { Date = reservation.Period.CheckOut, reservation.TotalPrice })
            .ToListAsync(cancellationToken);

        var revenueByDate = checkouts
            .GroupBy(item => item.Date)
            .ToDictionary(group => group.Key, group => new
            {
                Revenue = group.Sum(item => item.TotalPrice),
                Checkouts = group.Count(),
            });

        var csv = new CsvBuilder("Date", "Revenue", "Checkouts");
        for (var date = from; date <= to; date = date.AddDays(1))
        {
            var item = revenueByDate.GetValueOrDefault(date);
            csv.Add(date, item?.Revenue ?? 0m, item?.Checkouts ?? 0);
        }

        return new CsvReportDto($"stayflow-revenue-{from:yyyyMMdd}-{to:yyyyMMdd}.csv", csv.ToString());
    }
}

public sealed class GetArrivalsDeparturesCsvReportHandler(IApplicationDbContext dbContext, IDateTimeProvider clock)
    : IRequestHandler<GetArrivalsDeparturesCsvReportQuery, CsvReportDto>
{
    public async Task<CsvReportDto> Handle(GetArrivalsDeparturesCsvReportQuery request, CancellationToken cancellationToken)
    {
        var date = request.Date ?? DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);
        var rows = await dbContext.Reservations
            .AsNoTracking()
            .Where(reservation => reservation.Status != ReservationStatus.Cancelled
                && reservation.Status != ReservationStatus.NoShow
                && (reservation.Period.CheckIn == date || reservation.Period.CheckOut == date))
            .Join(dbContext.Guests,
                reservation => reservation.GuestId,
                guest => guest.Id,
                (reservation, guest) => new { reservation, guest })
            .Join(dbContext.Rooms,
                item => item.reservation.RoomId,
                room => room.Id,
                (item, room) => new
                {
                    Type = item.reservation.Period.CheckIn == date ? "Arrival" : "Departure",
                    item.reservation.ConfirmationCode,
                    GuestName = item.guest.FirstName + " " + item.guest.LastName,
                    RoomNumber = room.Number,
                    item.reservation.Status,
                    item.reservation.Period.CheckIn,
                    item.reservation.Period.CheckOut,
                })
            .OrderBy(row => row.Type)
            .ThenBy(row => row.RoomNumber)
            .ToListAsync(cancellationToken);

        var csv = new CsvBuilder("Type", "Confirmation Code", "Guest", "Room", "Status", "Check In", "Check Out");
        foreach (var row in rows)
        {
            csv.Add(row.Type, row.ConfirmationCode, row.GuestName, row.RoomNumber, row.Status, row.CheckIn, row.CheckOut);
        }

        return new CsvReportDto($"stayflow-arrivals-departures-{date:yyyyMMdd}.csv", csv.ToString());
    }
}

public sealed class GetNightAuditCsvReportHandler(IApplicationDbContext dbContext, IDateTimeProvider clock)
    : IRequestHandler<GetNightAuditCsvReportQuery, CsvReportDto>
{
    public async Task<CsvReportDto> Handle(GetNightAuditCsvReportQuery request, CancellationToken cancellationToken)
    {
        var date = request.Date ?? DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);
        var totalRooms = await dbContext.Rooms.CountAsync(cancellationToken);
        var occupiedRooms = await dbContext.Reservations
            .Where(reservation => reservation.Status == ReservationStatus.CheckedIn
                || (reservation.Status == ReservationStatus.CheckedOut && reservation.Period.CheckOut == date))
            .Select(reservation => reservation.RoomId)
            .Distinct()
            .CountAsync(cancellationToken);
        var arrivals = await dbContext.Reservations.CountAsync(reservation => reservation.Period.CheckIn == date, cancellationToken);
        var departures = await dbContext.Reservations.CountAsync(reservation => reservation.Period.CheckOut == date, cancellationToken);
        var checkedOutRevenue = await dbContext.Reservations
            .Where(reservation => reservation.Status == ReservationStatus.CheckedOut && reservation.Period.CheckOut == date)
            .SumAsync(reservation => (decimal?)reservation.TotalPrice, cancellationToken) ?? 0m;
        var dayStart = new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var dayEnd = dayStart.AddDays(1);
        var paidInvoices = await dbContext.Invoices
            .Include(invoice => invoice.LineItems)
            .Where(invoice => invoice.PaidAtUtc >= dayStart && invoice.PaidAtUtc < dayEnd)
            .ToListAsync(cancellationToken);
        var paidRevenue = paidInvoices.Sum(invoice => invoice.Total);
        var dirtyRooms = await dbContext.Rooms.CountAsync(room => room.CleaningStatus == RoomCleaningStatus.Dirty, cancellationToken);
        var outOfServiceRooms = await dbContext.Rooms.CountAsync(room => room.Status == RoomStatus.Maintenance || room.Status == RoomStatus.OutOfService, cancellationToken);
        var openHousekeeping = await dbContext.HousekeepingTasks.CountAsync(task => task.Status != HousekeepingTaskStatus.Completed, cancellationToken);
        var openMaintenance = await dbContext.WorkOrders.CountAsync(workOrder => workOrder.Status == WorkOrderStatus.Open || workOrder.Status == WorkOrderStatus.InProgress, cancellationToken);

        var csv = new CsvBuilder("Metric", "Value");
        csv.Add("Date", date);
        csv.Add("Total Rooms", totalRooms);
        csv.Add("Occupied Rooms", occupiedRooms);
        csv.Add("Occupancy Rate", totalRooms == 0 ? 0m : Math.Round(occupiedRooms * 100m / totalRooms, 2));
        csv.Add("Arrivals", arrivals);
        csv.Add("Departures", departures);
        csv.Add("Checked Out Stay Revenue", checkedOutRevenue);
        csv.Add("Paid Invoice Revenue", paidRevenue);
        csv.Add("Dirty Rooms", dirtyRooms);
        csv.Add("Out Of Service Rooms", outOfServiceRooms);
        csv.Add("Open Housekeeping Tasks", openHousekeeping);
        csv.Add("Open Maintenance Work Orders", openMaintenance);

        return new CsvReportDto($"stayflow-night-audit-{date:yyyyMMdd}.csv", csv.ToString());
    }
}

internal sealed class CsvBuilder
{
    private readonly StringBuilder _builder = new();

    public CsvBuilder(params string[] headers) => Add(headers);

    public void Add(params object?[] values)
    {
        _builder.AppendLine(string.Join(",", values.Select(Escape)));
    }

    public override string ToString() => _builder.ToString();

    private static string Escape(object? value)
    {
        var text = value switch
        {
            null => string.Empty,
            DateOnly date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            decimal number => number.ToString(CultureInfo.InvariantCulture),
            double number => number.ToString(CultureInfo.InvariantCulture),
            float number => number.ToString(CultureInfo.InvariantCulture),
            _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty,
        };

        return text.Contains(',') || text.Contains('"') || text.Contains('\n')
            ? $"\"{text.Replace("\"", "\"\"")}\""
            : text;
    }
}
