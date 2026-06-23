using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using StayFlow.Application.Common.Abstractions;

namespace StayFlow.Application.Common.Behaviors;

/// <summary>
/// Logs each request with the resolved tenant and user, and records handler duration —
/// the spine of request-level observability before distributed tracing is added.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger,
    ICurrentUser currentUser,
    ITenantProvider tenantProvider) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        logger.LogInformation(
            "Handling {RequestName} for tenant {TenantId} by user {UserId}",
            requestName,
            tenantProvider.TenantId,
            currentUser.UserId);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await next(cancellationToken);
            stopwatch.Stop();
            logger.LogInformation("Handled {RequestName} in {ElapsedMilliseconds}ms", requestName, stopwatch.ElapsedMilliseconds);
            return response;
        }
        catch (Exception exception)
        {
            stopwatch.Stop();
            logger.LogWarning(
                exception,
                "{RequestName} failed after {ElapsedMilliseconds}ms: {Message}",
                requestName,
                stopwatch.ElapsedMilliseconds,
                exception.Message);
            throw;
        }
    }
}
