using clean_architecture.application.Abstractions.Messaging;
using clean_architecture.application.Features.Notes.Get;
using clean_architecture.WebApi.Extensions;

namespace clean_architecture.WebApi.Endpoints.Notes;

internal sealed class Get : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/notes", async (IQueryHandler<GetNotesQuery, List<GetNotesResponse>> handler, CancellationToken cancellation) =>
        {
            var query = new GetNotesQuery();
            var results = await handler.Handle(query, cancellation);

            return results.Match(Results.Ok, Results.NotFound);

        })
        .WithTags(Tags.Notes)
        .WithName("GetNote")
        .WithDescription("Get all notes.")
        .WithSummary("Get all notes.")
        .Produces<List<GetNotesResponse>>(StatusCodes.Status200OK);
    }
}
