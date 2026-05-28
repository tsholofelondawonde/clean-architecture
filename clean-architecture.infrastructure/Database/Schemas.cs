namespace clean_architecture.infrastructure.Database;

internal static class Schemas
{
    public const string SqlServer = "dbo";
    public const string PostgreSql = "public";

    public static string GetDefaultSchema(string? providerName) =>
        providerName?.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) == true
            ? PostgreSql
            : SqlServer;
}
