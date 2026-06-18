using clean_architecture.WebApi.Extensions;
using clean_architecture.WebApi.Infrastructure;
using clean_architecture.WebApi.Middleware;
using System.Reflection;

namespace clean_architecture.WebApi;


/// <summary>
/// Provides extension methods for registering presentation layer services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers presentation layer services including controllers, exception handling, problem details, and middleware.
    /// Note: OpenAPI/Swagger configuration is handled separately via AddModernOpenApi extension method.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The updated <see cref="IServiceCollection\"/> instance.</returns>
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddControllers();

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        services.AddScoped<RequestContextLoggingMiddleware>();

        // Register all endpoints from this assembly
        services.AddEndpoints(typeof(DependencyInjection).Assembly);

        return services;
    }
}
