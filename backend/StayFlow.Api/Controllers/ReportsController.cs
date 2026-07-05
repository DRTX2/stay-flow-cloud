using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayFlow.Application.Common.Authorization;
using StayFlow.Application.Features.Reports;

namespace StayFlow.Api.Controllers;

[Authorize(Policy = Permissions.AnalyticsView)]
public sealed class ReportsController : ApiControllerBase
{
    [HttpGet("occupancy.csv")]
    public async Task<IActionResult> OccupancyCsv([FromQuery] DateOnly? from = null, [FromQuery] DateOnly? to = null)
        => Csv(await Sender.Send(new GetOccupancyCsvReportQuery(from, to)));

    [HttpGet("revenue.csv")]
    public async Task<IActionResult> RevenueCsv([FromQuery] int days = 30)
        => Csv(await Sender.Send(new GetRevenueCsvReportQuery(days)));

    [HttpGet("arrivals-departures.csv")]
    public async Task<IActionResult> ArrivalsDeparturesCsv([FromQuery] DateOnly? date = null)
        => Csv(await Sender.Send(new GetArrivalsDeparturesCsvReportQuery(date)));

    [HttpGet("night-audit.csv")]
    public async Task<IActionResult> NightAuditCsv([FromQuery] DateOnly? date = null)
        => Csv(await Sender.Send(new GetNightAuditCsvReportQuery(date)));

    private FileContentResult Csv(CsvReportDto report)
        => File(Encoding.UTF8.GetBytes(report.Content), report.ContentType, report.FileName);
}
