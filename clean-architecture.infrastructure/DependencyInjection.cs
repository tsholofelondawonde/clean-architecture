using clean_architecture.application.Abstractions.Data;
using clean_architecture.infrastructure.Database;
using clean_architecture.infrastructure.DomainEvents;
using clean_architecture.infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;

namespace clean_architecture.infrastructure;

/// <summary>
/// Provides extension methods for registering infrastructure services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all infrastructure services, including database, authentication, authorization, and health checks.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration) =>
        services
            .AddServices()
            .AddDatabase(configuration)
            .AddHealthChecks(configuration);
     
    /// <summary>
    /// Registers core infrastructure services and repositories.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The updated service collection.</returns>
    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        services.AddTransient<IDomainEventsDispatcher, DomainEventsDispatcher>();

        return services;
    }

    /// <summary>
    /// Configures and registers the application's database context with connection resilience.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The updated service collection.</returns>
    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("Database");

        var databaseProvider = DatabaseProviderResolver.Resolve(configuration);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "No database connection string found. Ensure 'ConnectionStrings:Database' is configured.");
        }

        services.AddDbContext<ApplicationDbContext>(
            options =>
            {
                switch (databaseProvider)
                {
                    case DatabaseProvider.SqlServer:
                        options.UseSqlServer(connectionString, sqlOptions =>
                        {
                            sqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.SqlServer);

                            // Add connection resilience with retry policy.
                            sqlOptions.EnableRetryOnFailure(
                                maxRetryCount: 3,
                                maxRetryDelay: TimeSpan.FromSeconds(5),
                                errorNumbersToAdd: null);

                            // Set command timeout to 60 seconds for complex queries.
                            sqlOptions.CommandTimeout(60);
                        });
                        break;

                    case DatabaseProvider.PostgreSql:
                        options.UseNpgsql(connectionString, npgsqlOptions =>
                        {
                            npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.PostgreSql);
                            npgsqlOptions.CommandTimeout(60);
                        });
                        break;
                }

                options
                    .EnableSensitiveDataLogging(false) // Disable for production.
                    .EnableDetailedErrors(false);       // Disable for production.
            }
        );

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        return services;
    }

    /// <summary>
    /// Registers health check services for the application.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The updated service collection.</returns>
    private static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database");

        var databaseProvider = DatabaseProviderResolver.Resolve(configuration);

        var healthChecksBuilder = services.AddHealthChecks();

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            switch (databaseProvider)
            {
                case DatabaseProvider.SqlServer:
                    healthChecksBuilder.AddSqlServer(connectionString);
                    break;
                case DatabaseProvider.PostgreSql:
                    healthChecksBuilder.AddNpgSql(connectionString);
                    break;
            }
        }

        return services;
    }
}