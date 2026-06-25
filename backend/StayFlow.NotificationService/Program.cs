using MassTransit;
using Serilog;
using StayFlow.NotificationService.Consumers;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .WriteTo.Console());

// The notification service is a pure bus consumer. It binds its own queue to the shared
// DomainEventOccurred contract; the message URN is derived from the type's namespace + name, which
// is why the contract lives in the shared StayFlow.Contracts assembly referenced by both sides.
var rabbitMq = builder.Configuration.GetConnectionString("RabbitMq");

builder.Services.AddMassTransit(configurator =>
{
    configurator.SetKebabCaseEndpointNameFormatter();
    configurator.AddConsumer<DomainEventNotificationConsumer>();

    if (!string.IsNullOrWhiteSpace(rabbitMq))
    {
        configurator.UsingRabbitMq((registration, cfg) =>
        {
            cfg.Host(new Uri(rabbitMq));
            cfg.ConfigureEndpoints(registration);
        });
    }
    else
    {
        // Lets the service boot standalone (e.g. local smoke tests) without a broker.
        configurator.UsingInMemory((registration, cfg) => cfg.ConfigureEndpoints(registration));
    }
});

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseSerilogRequestLogging();
app.MapHealthChecks("/health");

app.Run();
