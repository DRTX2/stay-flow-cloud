using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using StayFlow.Application.Common.Behaviors;
using StayFlow.Application.Pricing;

namespace StayFlow.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(assembly);
            configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        services.AddSingleton<IPricingService, DynamicPricingService>();

        return services;
    }
}
