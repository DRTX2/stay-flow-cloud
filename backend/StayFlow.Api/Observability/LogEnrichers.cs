using System.Diagnostics;
using Serilog.Core;
using Serilog.Events;

namespace StayFlow.Api.Observability;

public sealed class ActivityLogEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var activity = Activity.Current;
        if (activity is null)
        {
            return;
        }

        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("trace_id", activity.TraceId.ToHexString()));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("span_id", activity.SpanId.ToHexString()));
    }
}

public sealed class SensitiveDataLogEnricher : ILogEventEnricher
{
    private static readonly string[] _sensitiveNames =
        ["password", "secret", "token", "authorization", "cookie", "email", "recipient", "phone"];

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var sensitiveProperties = logEvent.Properties.Keys
            .Where(key => _sensitiveNames.Any(name => key.Contains(name, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        foreach (var propertyName in sensitiveProperties)
        {
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(propertyName, "[REDACTED]"));
        }
    }
}
