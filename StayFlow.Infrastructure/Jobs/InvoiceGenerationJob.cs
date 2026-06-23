using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Domain.Reservations;
using StayFlow.Persistence;

namespace StayFlow.Infrastructure.Jobs;

/// <summary>
/// Finds checked-out reservations that still have no invoice and would generate one. Currently
/// reports the backlog; the hook to issue invoices through the billing command is marked below.
/// </summary>
[AutomaticRetry(Attempts = 3)]
public sealed class InvoiceGenerationJob(
    StayFlowDbContext dbContext,
    ILogger<InvoiceGenerationJob> logger)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var awaitingInvoice = await dbContext.Reservations
            .IgnoreQueryFilters()
            .CountAsync(
                reservation => !reservation.IsDeleted
                    && reservation.Status == ReservationStatus.CheckedOut
                    && !dbContext.Invoices
                        .IgnoreQueryFilters()
                        .Any(invoice => invoice.ReservationId == reservation.Id && !invoice.IsDeleted),
                cancellationToken);

        logger.LogInformation(
            "Invoice generation: {Count} checked-out reservation(s) awaiting an invoice",
            awaitingInvoice);

        // TODO: issue an invoice per reservation via the billing command (GenerateInvoice).
    }
}
