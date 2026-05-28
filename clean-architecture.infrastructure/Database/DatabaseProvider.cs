using Microsoft.Extensions.Configuration;

namespace clean_architecture.infrastructure.Database;

internal enum DatabaseProvider
{
    SqlServer,
    PostgreSql
}

internal static class DatabaseProviderResolver
{
    private const string ProviderConfigurationPath = "Database:Provider";
    private const string DefaultProvider = nameof(DatabaseProvider.SqlServer);

    public static DatabaseProvider Resolve(IConfiguration configuration)
    {
        var providerValue = configuration[ProviderConfigurationPath] ?? DefaultProvider;

        if (Enum.TryParse(providerValue, ignoreCase: true, out DatabaseProvider provider))
        {
            return provider;
        }

        throw new InvalidOperationException(
            $"Invalid database provider '{providerValue}'. Supported values are '{DatabaseProvider.SqlServer}' and '{DatabaseProvider.PostgreSql}'.");
    }
}