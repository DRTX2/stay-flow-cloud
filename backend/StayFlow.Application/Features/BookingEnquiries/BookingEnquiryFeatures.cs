using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Application.Common.Exceptions;
using StayFlow.Application.Common.Models;
using StayFlow.Application.Features.Reservations;
using StayFlow.Application.Features.Reservations.Commands;
using StayFlow.Application.Pricing;
using StayFlow.Domain.BookingEnquiries;
using StayFlow.Domain.Guests;
using StayFlow.Domain.Reservations;
using StayFlow.Domain.Rooms;
using ValidationException = StayFlow.Application.Common.Exceptions.ValidationException;

namespace StayFlow.Application.Features.BookingEnquiries;

public sealed record BookingEnquiryDto(
    Guid Id,
    string Reference,
    Guid RoomTypeId,
    string RoomTypeName,
    DateOnly CheckIn,
    DateOnly CheckOut,
    int NumberOfGuests,
    string FullName,
    string Email,
    string? Phone,
    BookingEnquiryStatus Status,
    string? RejectionReason,
    Guid? ReservationId,
    DateTimeOffset CreatedAtUtc);

public sealed record CreateBookingEnquiryCommand(
    string HotelSlug,
    Guid RoomTypeId,
    DateOnly CheckIn,
    DateOnly CheckOut,
    int NumberOfGuests,
    string FullName,
    string Email,
    string? Phone) : IRequest<BookingEnquiryReceipt>;

public sealed record BookingEnquiryReceipt(string Reference, string Status);

public sealed class CreateBookingEnquiryValidator : AbstractValidator<CreateBookingEnquiryCommand>
{
    public CreateBookingEnquiryValidator()
    {
        RuleFor(x => x.HotelSlug).NotEmpty().MaximumLength(100);
        RuleFor(x => x.RoomTypeId).NotEmpty();
        RuleFor(x => x.CheckIn).GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow));
        RuleFor(x => x.CheckOut).GreaterThan(x => x.CheckIn);
        RuleFor(x => x.NumberOfGuests).InclusiveBetween(1, 20);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Phone).MaximumLength(40);
    }
}

public sealed class CreateBookingEnquiryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<CreateBookingEnquiryCommand, BookingEnquiryReceipt>
{
    public async Task<BookingEnquiryReceipt> Handle(CreateBookingEnquiryCommand request, CancellationToken cancellationToken)
    {
        var slug = request.HotelSlug.Trim().ToLowerInvariant();
        var tenant = await dbContext.Tenants.AsNoTracking()
            .SingleOrDefaultAsync(t => t.Slug == slug && t.IsActive, cancellationToken)
            ?? throw new ValidationException([new ValidationFailure(nameof(request.HotelSlug), "The selected hotel is unavailable.")]);

        var roomType = await dbContext.RoomTypes.IgnoreQueryFilters().AsNoTracking()
            .SingleOrDefaultAsync(
                rt => rt.Id == request.RoomTypeId && rt.TenantId == tenant.Id && !rt.IsDeleted,
                cancellationToken)
            ?? throw new ValidationException([new ValidationFailure(nameof(request.RoomTypeId), "The selected room type is unavailable at this hotel.")]);

        if (request.NumberOfGuests > roomType.MaxOccupancy)
        {
            throw new ValidationException(
                [new ValidationFailure(nameof(request.NumberOfGuests), $"This room type holds at most {roomType.MaxOccupancy} guests.")]);
        }

        var activeStatuses = new[] { ReservationStatus.Pending, ReservationStatus.Confirmed, ReservationStatus.CheckedIn };
        var hasAvailableRoom = await dbContext.Rooms.IgnoreQueryFilters().AsNoTracking()
            .AnyAsync(room => room.TenantId == tenant.Id
                              && room.RoomTypeId == roomType.Id
                              && !room.IsDeleted
                              && room.Capacity >= request.NumberOfGuests
                              && room.Status != RoomStatus.Maintenance
                              && room.Status != RoomStatus.OutOfService
                              && !dbContext.Reservations.IgnoreQueryFilters().Any(reservation =>
                                  reservation.TenantId == tenant.Id
                                  && reservation.RoomId == room.Id
                                  && !reservation.IsDeleted
                                  && activeStatuses.Contains(reservation.Status)
                                  && reservation.Period.CheckIn < request.CheckOut
                                  && request.CheckIn < reservation.Period.CheckOut), cancellationToken);
        if (!hasAvailableRoom)
        {
            throw new ValidationException(
                [new ValidationFailure(nameof(request.RoomTypeId), "No rooms are available for the selected dates and party size.")]);
        }

        var enquiry = BookingEnquiry.Create(
            tenant.Id,
            roomType.Id,
            request.CheckIn,
            request.CheckOut,
            request.NumberOfGuests,
            request.FullName,
            request.Email,
            request.Phone);

        dbContext.BookingEnquiries.Add(enquiry);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new BookingEnquiryReceipt(enquiry.Reference, "received");
    }
}

public sealed record GetBookingEnquiriesQuery(int Page = 1, int PageSize = 20, BookingEnquiryStatus? Status = null)
    : IRequest<PagedResult<BookingEnquiryDto>>;

public sealed class GetBookingEnquiriesHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetBookingEnquiriesQuery, PagedResult<BookingEnquiryDto>>
{
    public async Task<PagedResult<BookingEnquiryDto>> Handle(GetBookingEnquiriesQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var query = dbContext.BookingEnquiries.AsNoTracking();
        if (request.Status is { } status)
        {
            query = query.Where(e => e.Status == status);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(e => e.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Join(
                dbContext.RoomTypes.AsNoTracking(),
                enquiry => enquiry.RoomTypeId,
                roomType => roomType.Id,
                (enquiry, roomType) => new BookingEnquiryDto(
                    enquiry.Id,
                    enquiry.Reference,
                    enquiry.RoomTypeId,
                    roomType.Name,
                    enquiry.CheckIn,
                    enquiry.CheckOut,
                    enquiry.NumberOfGuests,
                    enquiry.FullName,
                    enquiry.Email,
                    enquiry.Phone,
                    enquiry.Status,
                    enquiry.RejectionReason,
                    enquiry.ReservationId,
                    enquiry.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return new PagedResult<BookingEnquiryDto>(items, page, pageSize, totalCount);
    }
}

public sealed record RejectBookingEnquiryCommand(Guid Id, string? Reason) : IRequest;

public sealed class RejectBookingEnquiryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<RejectBookingEnquiryCommand>
{
    public async Task Handle(RejectBookingEnquiryCommand request, CancellationToken cancellationToken)
    {
        var enquiry = await dbContext.BookingEnquiries.SingleOrDefaultAsync(e => e.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(BookingEnquiry), request.Id);
        enquiry.Reject(request.Reason);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

public sealed record ConvertBookingEnquiryCommand(Guid Id, Guid RoomId) : IRequest<ReservationDto>;

public sealed class ConvertBookingEnquiryHandler(IApplicationDbContext dbContext, IPricingService pricingService)
    : IRequestHandler<ConvertBookingEnquiryCommand, ReservationDto>
{
    public async Task<ReservationDto> Handle(ConvertBookingEnquiryCommand request, CancellationToken cancellationToken)
    {
        var enquiry = await dbContext.BookingEnquiries.SingleOrDefaultAsync(e => e.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(BookingEnquiry), request.Id);

        if (enquiry.Status != BookingEnquiryStatus.Pending)
        {
            throw new ValidationException([new ValidationFailure(nameof(request.Id), "This enquiry has already been processed.")]);
        }

        var room = await dbContext.Rooms.SingleOrDefaultAsync(r => r.Id == request.RoomId, cancellationToken)
            ?? throw new ValidationException([new ValidationFailure(nameof(request.RoomId), "The selected room is unavailable.")]);
        if (room.RoomTypeId != enquiry.RoomTypeId || !room.IsBookable)
        {
            throw new ValidationException([new ValidationFailure(nameof(request.RoomId), "Select an available room of the requested type.")]);
        }

        if (enquiry.NumberOfGuests > room.Capacity)
        {
            throw new ValidationException([new ValidationFailure(nameof(request.RoomId), $"Room {room.Number} holds at most {room.Capacity} guests.")]);
        }

        if (await ReservationAvailability.HasOverlapAsync(dbContext, room.Id, enquiry.CheckIn, enquiry.CheckOut, cancellationToken))
        {
            throw new ReservationConflictException();
        }

        var guest = await dbContext.Guests.SingleOrDefaultAsync(g => g.Email == enquiry.Email, cancellationToken);
        if (guest is null)
        {
            var names = enquiry.FullName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            guest = Guest.Create(names[0], names.Length > 1 ? names[1] : "Guest", enquiry.Email, enquiry.Phone);
            dbContext.Guests.Add(guest);
        }

        var occupancy = await ReservationAvailability.OccupancyAsync(
            dbContext, enquiry.CheckIn, enquiry.CheckOut, cancellationToken);
        var quote = pricingService.Quote(new PricingRequest(
            room.BasePrice,
            enquiry.CheckIn,
            enquiry.CheckOut,
            occupancy,
            enquiry.NumberOfGuests));

        var reservation = Reservation.Create(
            room.Id,
            guest.Id,
            DateRange.Create(enquiry.CheckIn, enquiry.CheckOut),
            enquiry.NumberOfGuests,
            quote.TotalPrice);
        dbContext.Reservations.Add(reservation);
        enquiry.MarkConverted(reservation.Id);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ReservationDto.FromEntity(reservation);
    }
}
