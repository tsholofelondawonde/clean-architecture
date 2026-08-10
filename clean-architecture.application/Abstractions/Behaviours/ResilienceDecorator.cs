using clean_architecture.application.Abstractions.Messaging;
using Polly;
using Polly.Registry;
using SharedKernel;

namespace clean_architecture.application.Abstractions.Behaviours;

/// <summary>
/// Provides resilience decorators for query handlers, retrying transient failures
/// (e.g. momentary database connectivity issues) that slip past EF Core's own execution strategy.
/// </summary>
internal static class ResilienceDecorator
{
    /// <summary>
    /// The key of the named resilience pipeline applied to query handlers.
    /// </summary>
    internal const string QueryPipelineKey = "queries";

    /// <summary>
    /// Decorator for <see cref="IQueryHandler{TQuery, TResponse}"/> that retries query processing
    /// on transient failures using a named resilience pipeline.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    internal sealed class QueryHandler<TQuery, TResponse>(
        IQueryHandler<TQuery, TResponse> innerHandler,
        ResiliencePipelineProvider<string> pipelineProvider)
        : IQueryHandler<TQuery, TResponse>
        where TQuery : IQuery<TResponse>
    {
        /// <summary>
        /// Handles the query within a resilience pipeline, retrying on transient failures.
        /// </summary>
        /// <param name="query">The query to handle.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The result of the query execution.</returns>
        public async Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken)
        {
            ResiliencePipeline pipeline = pipelineProvider.GetPipeline(QueryPipelineKey);

            try
            {
                return await pipeline.ExecuteAsync(
                    async token => await innerHandler.Handle(query, token),
                    cancellationToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                return Result.Failure<TResponse>(
                    Error.Failure(
                        "Resilience.QueryFailed",
                        $"The query {typeof(TQuery).Name} failed after retrying: {exception.Message}"));
            }
        }
    }
}
