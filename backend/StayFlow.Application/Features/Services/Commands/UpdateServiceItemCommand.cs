using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Application.Common.Exceptions;
using StayFlow.Domain.Services;

namespace StayFlow.Application.Features.Services.Commands;

public sealed record UpdateServiceItemCommand(
    Guid Id,
    string Name,
    decimal Price,
    ServiceCategory Category,
    string? Description = null) : IRequest;

public sealed class UpdateServiceItemValidator : AbstractValidator<UpdateServiceItemCommand>
{
    public UpdateServiceItemValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Category).IsInEnum();
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}

public sealed class UpdateServiceItemHandler(IApplicationDbContext dbContext)
    : IRequestHandler<UpdateServiceItemCommand>
{
    public async Task Handle(UpdateServiceItemCommand request, CancellationToken cancellationToken)
    {
        var service = await dbContext.ServiceItems.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(ServiceItem), request.Id);

        service.Update(request.Name, request.Price, request.Category, request.Description);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
