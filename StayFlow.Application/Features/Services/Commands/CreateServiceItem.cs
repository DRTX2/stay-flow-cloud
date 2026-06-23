using FluentValidation;
using MediatR;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Domain.Services;

namespace StayFlow.Application.Features.Services.Commands;

public sealed record CreateServiceItemCommand(
    string Name,
    decimal Price,
    ServiceCategory Category,
    string? Description = null) : IRequest<Guid>;

public sealed class CreateServiceItemValidator : AbstractValidator<CreateServiceItemCommand>
{
    public CreateServiceItemValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Category).IsInEnum();
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}

public sealed class CreateServiceItemHandler(IApplicationDbContext dbContext)
    : IRequestHandler<CreateServiceItemCommand, Guid>
{
    public async Task<Guid> Handle(CreateServiceItemCommand request, CancellationToken cancellationToken)
    {
        var service = ServiceItem.Create(request.Name, request.Price, request.Category, request.Description);
        dbContext.ServiceItems.Add(service);
        await dbContext.SaveChangesAsync(cancellationToken);
        return service.Id;
    }
}
