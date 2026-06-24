using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Application.Common.Exceptions;
using StayFlow.Domain.Billing;
using StayFlow.Domain.Reservations;
using ValidationException = StayFlow.Application.Common.Exceptions.ValidationException;

namespace StayFlow.Application.Features.Billing.Commands;

/// <summary>
/// Generates and issues the invoice for a reservation: one accommodation line, a line per posted
/// service charge, then tax. A reservation can only be invoiced once.
/// </summary>
public sealed record GenerateInvoiceCommand(Guid ReservationId, decimal? TaxRate = null) : IRequest<InvoiceDto>;

public sealed class GenerateInvoiceValidator : AbstractValidator<GenerateInvoiceCommand>
{
    public GenerateInvoiceValidator()
    {
        RuleFor(x => x.ReservationId).NotEmpty();
        RuleFor(x => x.TaxRate).InclusiveBetween(0m, 1m).When(x => x.TaxRate.HasValue);
    }
}

public sealed class GenerateInvoiceHandler(IApplicationDbContext dbContext, IDateTimeProvider clock)
    : IRequestHandler<GenerateInvoiceCommand, InvoiceDto>
{
    private const decimal DefaultTaxRate = 0.10m;

    public async Task<InvoiceDto> Handle(GenerateInvoiceCommand request, CancellationToken cancellationToken)
    {
        var reservation = await dbContext.Reservations
            .FirstOrDefaultAsync(r => r.Id == request.ReservationId, cancellationToken)
            ?? throw new NotFoundException(nameof(Reservation), request.ReservationId);

        if (reservation.Status is ReservationStatus.Pending or ReservationStatus.Cancelled)
        {
            throw new ValidationException(
            [
                new FluentValidation.Results.ValidationFailure(
                    nameof(request.ReservationId),
                    $"A {reservation.Status} reservation cannot be invoiced."),
            ]);
        }

        var alreadyInvoiced = await dbContext.Invoices
            .AnyAsync(i => i.ReservationId == reservation.Id, cancellationToken);
        if (alreadyInvoiced)
        {
            throw new ValidationException(
            [
                new FluentValidation.Results.ValidationFailure(
                    nameof(request.ReservationId), "This reservation has already been invoiced."),
            ]);
        }

        var currency = await dbContext.Tenants
            .Where(t => t.Id == reservation.TenantId)
            .Select(t => t.DefaultCurrency)
            .FirstOrDefaultAsync(cancellationToken) ?? "USD";

        var number = $"INV-{clock.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
        var invoice = Invoice.Create(reservation.Id, number, currency, request.TaxRate ?? DefaultTaxRate);

        invoice.AddLine(
            LineItemKind.Accommodation,
            $"Accommodation ({reservation.Period.Nights} night(s))",
            quantity: 1,
            unitPrice: reservation.TotalPrice);

        var charges = await dbContext.ReservationCharges
            .Where(c => c.ReservationId == reservation.Id)
            .ToListAsync(cancellationToken);
        foreach (var charge in charges)
        {
            invoice.AddLine(LineItemKind.Service, charge.Description, charge.Quantity, charge.UnitPrice);
        }

        invoice.ApplyTax();
        invoice.Issue(clock.UtcNow);

        dbContext.Invoices.Add(invoice);
        await dbContext.SaveChangesAsync(cancellationToken);

        return InvoiceDto.FromEntity(invoice);
    }
}
