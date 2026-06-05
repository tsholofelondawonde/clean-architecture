using clean_architecture.application.Abstractions.Behaviours;
using clean_architecture.application.Abstractions.Messaging;
using Microsoft.Extensions.Configuration;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;

namespace clean_architecture.application;

/// <summary>
/// Provides extension methods for registering application services and handlers in the dependency injection container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers application services, handlers, decorators, and validators in the dependency injection container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> instance for accessing configuration settings.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        if (services is null)
            throw new ArgumentNullException(nameof(services));

        if (configuration is null)
            throw new ArgumentNullException(nameof(configuration));

        var assembly = typeof(DependencyInjection).Assembly;

        // Register query, command and domain event handlers from this assembly
        services.Scan(scan => scan.FromAssemblies(assembly)
            .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<,>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(IDomainEventHandler<>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        // Apply validation decorators first so logging wraps validated handlers
        services.Decorate(typeof(ICommandHandler<,>), typeof(ValidationDecorator.CommandHandler<,>));

        // Apply logging decorators
        services.Decorate(typeof(IQueryHandler<,>), typeof(LoggingDecorator.QueryHandler<,>));
        services.Decorate(typeof(ICommandHandler<,>), typeof(LoggingDecorator.CommandHandler<,>));

        // Only decorate ICommandHandler<> (no-result variant) if implementations exist
        if (services.Any(d => d.ServiceType == typeof(ICommandHandler<>)))
        {
            services.Decorate(typeof(ICommandHandler<>), typeof(ValidationDecorator.CommandBaseHandler<>));
            services.Decorate(typeof(ICommandHandler<>), typeof(LoggingDecorator.CommandBaseHandler<>));
        }

        // Register validators from this assembly (including internal types)
        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        return services;
    }
}
