using clean_architecture.application.Abstractions.Messaging;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace clean_architecture.application.Abstractions.Behaviours;

/// <summary>
/// Carries a failed <see cref="Result{TValue}"/> out of a HybridCache factory delegate
/// without letting HybridCache persist the failure as a cached entry.
/// </summary>
public sealed class CacheableQueryFailedException : Exception
{
    public CacheableQueryFailedException()
    {
    }

    public CacheableQueryFailedException(string message)
        : base(message)
    {
    }

    public CacheableQueryFailedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public CacheableQueryFailedException(object failureResult)
        : base("The query failed and its result will not be cached.")
    {
        FailureResult = failureResult;
    }

    public object? FailureResult { get; }
}

/// <summary>
/// Provides a caching decorator for query handlers, serving cached results via
/// <see cref="HybridCache"/> for queries that opt in via <see cref="ICachedQuery"/>.
/// </summary>
internal static class CachingDecorator
{
    /// <summary>
    /// Decorator for <see cref="IQueryHandler{TQuery, TResponse}"/> that serves cached results
    /// for queries implementing <see cref="ICachedQuery"/>, populating the cache on miss.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    internal sealed class QueryHandler<TQuery, TResponse>(
        IQueryHandler<TQuery, TResponse> innerHandler,
        HybridCache cache,
        ILogger<QueryHandler<TQuery, TResponse>> logger)
        : IQueryHandler<TQuery, TResponse>
        where TQuery : IQuery<TResponse>
    {
        /// <summary>
        /// Handles the query, serving a cached result when available and otherwise
        /// invoking the inner handler and caching a successful result.
        /// </summary>
        /// <param name="query">The query to handle.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The result of the query execution.</returns>
        public async Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken)
        {
            if (query is not ICachedQuery cachedQuery)
            {
                return await innerHandler.Handle(query, cancellationToken);
            }

            return await HandleCached(query, cachedQuery, cancellationToken);
        }

        private async Task<Result<TResponse>> HandleCached(
            TQuery query,
            ICachedQuery cachedQuery,
            CancellationToken cancellationToken)
        {
            var entryOptions = BuildEntryOptions(cachedQuery);
            var wasCached = true;

            try
            {
                Result<TResponse> result = await cache.GetOrCreateAsync(
                    cachedQuery.CacheKey,
                    async token =>
                    {
                        wasCached = false;
                        return await InvokeAndGuardAgainstCachingFailure(query, token);
                    },
                    entryOptions,
                    tags: cachedQuery.Tags,
                    cancellationToken: cancellationToken);

                LogOutcome(cachedQuery.CacheKey, wasCached);

                return result;
            }
            catch (CacheableQueryFailedException exception)
            {
                logger.LogDebug(exception, "Cache miss for {CacheKey}, handler failed", cachedQuery.CacheKey);

                return (Result<TResponse>)exception.FailureResult!;
            }
        }

        private async Task<Result<TResponse>> InvokeAndGuardAgainstCachingFailure(TQuery query, CancellationToken cancellationToken)
        {
            Result<TResponse> innerResult = await innerHandler.Handle(query, cancellationToken);

            if (innerResult.IsFailure)
            {
                throw new CacheableQueryFailedException(innerResult);
            }

            return innerResult;
        }

        private void LogOutcome(string cacheKey, bool wasCached)
        {
            if (wasCached)
            {
                logger.LogDebug("Cache hit for {CacheKey}", cacheKey);
            }
            else
            {
                logger.LogDebug("Cache miss for {CacheKey}, invoked handler", cacheKey);
            }
        }

        private static HybridCacheEntryOptions? BuildEntryOptions(ICachedQuery cachedQuery) =>
            cachedQuery.Expiration is { } expiration
                ? new HybridCacheEntryOptions
                {
                    Expiration = expiration,
                    LocalCacheExpiration = expiration,
                }
                : null;
    }
}
