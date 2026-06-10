using System.Text.Json;
using FluentValidation;

namespace EventService.WebAPI.Middleware;

/// <summary>
/// Translates domain/validation exceptions into RFC7807-style problem responses so
/// controllers stay thin and clients get consistent error payloads.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            await WriteProblem(context, StatusCodes.Status400BadRequest, "Validation failed",
                ex.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));
        }
        catch (ArgumentException ex)
        {
            await WriteProblem(context, StatusCodes.Status400BadRequest, "Invalid request", new[] { ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception processing {Path}", context.Request.Path);
            await WriteProblem(context, StatusCodes.Status500InternalServerError, "Unexpected error",
                new[] { "An unexpected error occurred." });
        }
    }

    private static async Task WriteProblem(HttpContext context, int status, string title, IEnumerable<string> errors)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = status;

        var payload = new
        {
            title,
            status,
            errors = errors.ToArray(),
            traceId = context.TraceIdentifier
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
