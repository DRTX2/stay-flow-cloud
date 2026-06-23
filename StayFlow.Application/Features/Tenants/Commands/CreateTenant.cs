using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Application.Common.Exceptions;
using StayFlow.Domain.Tenants;
using ValidationException = StayFlow.Application.Common.Exceptions.ValidationException;

namespace StayFlow.Application.Features.Tenants.Commands;

public sealed record CreateTenantCommand(
    string Name,
    string Slug,
    PropertyType PropertyType,
    string DefaultCurrency = "USD") : IRequest<Guid>;

public sealed class CreateTenantValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug)
            .NotEmpty()
            .MaximumLength(100)
            .Matches("^[a-z0-9-]+$").WithMessage("Slug may contain only lowercase letters, digits and hyphens.");
        RuleFor(x => x.DefaultCurrency).NotEmpty().Length(3);
        RuleFor(x => x.PropertyType).IsInEnum();
    }
}

public sealed class CreateTenantHandler(IApplicationDbContext dbContext)
    : IRequestHandler<CreateTenantCommand, Guid>
{
    public async Task<Guid> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        var slug = request.Slug.Trim().ToLowerInvariant();
        var slugTaken = await dbContext.Tenants.AnyAsync(t => t.Slug == slug, cancellationToken);
        if (slugTaken)
        {
            throw new ValidationException(
            [
                new FluentValidation.Results.ValidationFailure(nameof(request.Slug), $"Slug '{slug}' is already in use."),
            ]);
        }

        var tenant = Tenant.Create(request.Name, slug, request.PropertyType, request.DefaultCurrency);
        dbContext.Tenants.Add(tenant);
        await dbContext.SaveChangesAsync(cancellationToken);

        return tenant.Id;
    }
}
