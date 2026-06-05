using clean_architecture.WebApi.Middleware;

namespace clean_architecture.WebApi.Extensions;


/// <summary>
/// Provides extension methods for registering custom middleware in the application pipeline.
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>
    /// Adds the <see cref="RequestContextLoggingMiddleware"/> to the application's request pipeline.
    /// This middleware enriches Serilog logs with a correlation ID for each request.
    /// </summary>
    /// <param name="app">The <see cref="IApplicationBuilder"/> to add the middleware to.</param>
    /// <returns>The <see cref="IApplicationBuilder"/> for chaining.</returns>
    public static IApplicationBuilder UseRequestContextLogging(this IApplicationBuilder app)
    {
        app.UseMiddleware<RequestContextLoggingMiddleware>();

        return app;
    }
}
