namespace clean_architecture.WebApi.Extensions;


/// <summary>
/// Provides extension methods for configuring services in the application.
/// </summary>
internal static class ServiceCollectionExtensions
{
    private const string DomainNamespace = "clean_architecture.Domain";
    private const string SharedKernelNamespace = "SharedKernel";

    /// <summary>
    /// Checks if a type is from the Domain layer (should not be serialized to JSON).
    /// </summary>
    private static bool IsDomainType(Type type)
    {
        // Handle nullable types
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        // Handle generic types (e.g., List<Branch>)
        if (underlyingType.IsGenericType)
        {
            // Check if any generic argument is a domain type
            foreach (var genericArg in underlyingType.GetGenericArguments())
            {
                if (IsDomainType(genericArg))
                {
                    return true;
                }
            }
        }

        var typeNamespace = underlyingType.Namespace ?? string.Empty;

        return typeNamespace.StartsWith(DomainNamespace, StringComparison.Ordinal) ||
               typeNamespace.StartsWith(SharedKernelNamespace, StringComparison.Ordinal);
    }
}
