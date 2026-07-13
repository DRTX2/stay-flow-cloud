using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StayFlow.Application.Common.Exceptions;
using StayFlow.Domain.Common;

namespace StayFlow.Api.Middleware;

/// <summary>
/// Translates the application's exception vocabulary into RFC 7807 problem responses, so handlers
/// can throw domain/application exceptions and controllers stay free of try/catch noise.
/// </summary>
public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            await WriteProblemAsync(context, new ValidationProblemDetails(
                ex.Errors.ToDictionary(e => e.Key, e => e.Value))
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred.",
            });
        }
        catch (NotFoundException ex)
        {
            await WriteProblemAsync(context, Problem(StatusCodes.Status404NotFound, "Resource not found.", ex.Message));
        }
        catch (ReservationConflictException ex)
        {
            await WriteProblemAsync(context, Problem(StatusCodes.Status409Conflict, "Reservation conflict.", ex.Message));
        }
        catch (DomainException ex)
        {
            await WriteProblemAsync(context, Problem(StatusCodes.Status409Conflict, "Domain rule violated.", ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception processing {Method} {Path}", context.Request.Method, context.Request.Path);
            await WriteProblemAsync(context, Problem(
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.",
                "The request could not be processed."));
        }
    }

    private static ProblemDetails Problem(int status, string title, string detail) => new()
    {
        Status = status,
        Title = title,
        Detail = detail,
    };

    private static async Task WriteProblemAsync(HttpContext context, ProblemDetails problem)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        problem.Extensions["traceId"] = Activity.Current?.TraceId.ToHexString() ?? context.TraceIdentifier;
        context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problem, problem.GetType());
    }
}
