using Microsoft.Extensions.Primitives;
using Serilog.Context;

namespace clean_architecture.WebApi.Middleware;
/// <summary>
/// Middleware that enriches Serilog logs with a correlation ID from the request headers or trace identifier.
/// </summary>
public class RequestContextLoggingMiddleware : IMiddleware
{
    private const string CorrelationIdHeaderName = "Correlation-Id";

    /// <summary>
    /// Invokes the middleware, pushing the correlation ID into the Serilog log context for the current request.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        using (LogContext.PushProperty("CorrelationId", GetCorrelationId(context)))
        {
            await next.Invoke(context);
        }
    }

    /// <summary>
    /// Retrieves the correlation ID from the request headers, or falls back to the trace identifier if not present.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <returns>The correlation ID string.</returns>
    private static string GetCorrelationId(HttpContext context)
    {
        context.Request.Headers.TryGetValue(
            CorrelationIdHeaderName,
            out StringValues correlationId);

        return correlationId.FirstOrDefault() ?? context.TraceIdentifier;
    }
}
