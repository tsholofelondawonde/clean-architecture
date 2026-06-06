using clean_architecture.infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace clean_architecture.infrastructure.Factories;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Determine the environment (default to Development for design-time operations)
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        // Set the base path to the Web API project directory
        // Navigate up from Infrastructure project to the root, then to WebApi project
        var currentDirectory = Directory.GetCurrentDirectory();
        var basePath = Path.Combine(currentDirectory, "clean_architecture.WebApi");

        // If that doesn't exist, assume we're already in the WebApi directory or root
        if (!Directory.Exists(basePath))
        {
            basePath = currentDirectory;
        }

        // Build configuration with environment-specific overrides
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        // Get the connection string from the configuration
        var connectionString = configuration.GetConnectionString("LocalSQLDb")
            ?? configuration.GetConnectionString("LocalDb");

        var databaseProvider = DatabaseProviderResolver.Resolve(configuration);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"No database connection string found. Ensure either 'ConnectionStrings:ProdDb' or 'ConnectionStrings:LocalDb' exists in appsettings files. " +
                $"Base path: {basePath}. Environment: {environment}");
        }

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        switch (databaseProvider)
        {
            case DatabaseProvider.SqlServer:
                optionsBuilder.UseSqlServer(connectionString);
                break;
            case DatabaseProvider.PostgreSql:
                optionsBuilder.UseNpgsql(connectionString);
                break;
            default:
                throw new InvalidOperationException($"Unsupported database provider: {databaseProvider}");
        }

        // Log the configuration details for debugging
        Console.WriteLine($"[EF Design Time] Provider: {databaseProvider}");
        Console.WriteLine($"[EF Design Time] Environment: {environment}");
        Console.WriteLine($"[EF Design Time] Base Path: {basePath}");
        Console.WriteLine($"[EF Design Time] Connection String: {connectionString}");
        // Pass null for domainEventsDispatcher since it's not available at design time
        return new ApplicationDbContext(optionsBuilder.Options, null);
    }
}

