using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace StayFlow.Infrastructure.Messaging;

public static class MessagingServiceCollectionExtensions
{
    /// <summary>
    /// Registers the message bus and the outbox relay. MassTransit uses the in-memory transport for
    /// the modular monolith; the bus is configured the same way regardless of transport, so swapping
    /// in RabbitMQ or Amazon SQS later is a one-line change here. The <see cref="OutboxProcessor"/>
    /// hosted service publishes domain events captured by the transactional outbox.
    /// </summary>
    public static IServiceCollection AddMessaging(this IServiceCollection services)
    {
        services.AddMassTransit(configurator =>
        {
            configurator.AddConsumer<DomainEventOccurredConsumer>();

            configurator.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
        });

        services.AddHostedService<OutboxProcessor>();

        return services;
    }
}
