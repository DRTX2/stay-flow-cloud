using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using StayFlow.Application.Common.Auditing;
using StayFlow.Application.Common.Behaviors;
using StayFlow.Application.Common.Events;
using StayFlow.Application.Pricing;
using StayFlow.Domain.Common;

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

        RegisterAuditHandlers(services);

        return services;
    }

    /// <summary>
    /// Wires the generic <see cref="DomainEventAuditor{T}"/> as a notification handler for each
    /// concrete domain event, so every published event is recorded to the audit store.
    /// </summary>
    private static void RegisterAuditHandlers(IServiceCollection services)
    {
        var domainEventTypes = typeof(IDomainEvent).Assembly.GetTypes()
            .Where(type => type is { IsAbstract: false, IsInterface: false } && typeof(IDomainEvent).IsAssignableFrom(type));

        foreach (var eventType in domainEventTypes)
        {
            var notificationType = typeof(DomainEventNotification<>).MakeGenericType(eventType);
            var handlerServiceType = typeof(INotificationHandler<>).MakeGenericType(notificationType);
            var handlerImplementationType = typeof(DomainEventAuditor<>).MakeGenericType(eventType);
            services.AddTransient(handlerServiceType, handlerImplementationType);
        }
    }
}
