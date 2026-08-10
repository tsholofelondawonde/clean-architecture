namespace clean_architecture.application.Abstractions.Messaging;

/// <summary>
/// Marks a query as cacheable via HybridCache, opted into by implementing this
/// alongside <see cref="IQuery{TResponse}"/>.
/// </summary>
public interface ICachedQuery
{
    /// <summary>
    /// The cache key under which the query's result is stored.
    /// </summary>
    string CacheKey { get; }

    /// <summary>
    /// The expiration for this entry, or <see langword="null"/> to use HybridCache's configured default.
    /// </summary>
    TimeSpan? Expiration { get; }

    /// <summary>
    /// Tags applied to the cache entry, used for bulk invalidation.
    /// </summary>
    IReadOnlyCollection<string> Tags { get; }
}
