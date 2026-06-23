using MediatR;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Domain.Tenants;
using ValidationException = StayFlow.Application.Common.Exceptions.ValidationException;

namespace StayFlow.Application.Features.Tenants.Commands;

/// <summary>Overrides a feature for the current tenant. A feature can only be enabled if the
/// tenant's plan makes it available; disabling is always allowed.</summary>
public sealed record SetTenantFeatureCommand(Feature Feature, bool Enabled) : IRequest;

public sealed class SetTenantFeatureHandler(IApplicationDbContext dbContext, IFeatureService features)
    : IRequestHandler<SetTenantFeatureCommand>
{
    public async Task Handle(SetTenantFeatureCommand request, CancellationToken cancellationToken)
    {
        if (request.Enabled)
        {
            var limits = await features.GetLimitsAsync(cancellationToken);
            if (!limits.Includes(request.Feature))
            {
                throw new ValidationException(
                [
                    new FluentValidation.Results.ValidationFailure(
                        nameof(request.Feature),
                        $"Feature '{request.Feature}' is not available on your current plan."),
                ]);
            }
        }

        var existing = await dbContext.TenantFeatureOverrides
            .FirstOrDefaultAsync(o => o.Feature == request.Feature, cancellationToken);

        if (existing is null)
        {
            dbContext.TenantFeatureOverrides.Add(TenantFeatureOverride.Create(request.Feature, request.Enabled));
        }
        else
        {
            existing.Set(request.Enabled);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
