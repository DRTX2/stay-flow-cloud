using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace StayFlow.Infrastructure.Messaging;

public static class MessagingServiceCollectionExtensions
{
    /// <summary>
    /// Registers the message bus and the outbox relay. When a RabbitMQ connection string is
    /// configured (<c>ConnectionStrings:RabbitMq</c>) the bus runs over RabbitMQ so events cross
    /// process boundaries to the extracted microservices; otherwise it falls back to the in-memory
    /// transport for the standalone modular monolith. Producers/consumers are identical either way —
    /// only the transport line changes. The <see cref="OutboxProcessor"/> hosted service publishes
    /// domain events captured by the transactional outbox.
    /// </summary>
    public static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var rabbitMq = configuration.GetConnectionString("RabbitMq");

        services.AddMassTransit(configurator =>
        {
            configurator.AddConsumer<DomainEventOccurredConsumer>();

            if (!string.IsNullOrWhiteSpace(rabbitMq))
            {
                configurator.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(new Uri(rabbitMq));
                    cfg.ConfigureEndpoints(context);
                });
            }
            else
            {
                configurator.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
            }
        });

        services.AddHostedService<OutboxProcessor>();

        return services;
    }
}
