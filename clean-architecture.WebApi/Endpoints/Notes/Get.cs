using clean_architecture.application.Abstractions.Messaging;
using clean_architecture.application.Notes.Get;
using clean_architecture.contracts.Notes;
using clean_architecture.WebApi.Extensions;

namespace clean_architecture.WebApi.Endpoints.Notes;

internal sealed class Get : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/notes/{id:guid}", async (Guid id, IQueryHandler<GetNoteQuery, List<NoteResponse>>
        handler, CancellationToken cancellation) =>
        {
            var query = new GetNoteQuery();
            var results = await handler.Handle(query, cancellation);

            return results.Match(Results.Ok, Results.NotFound);

        })
        .WithTags(Tags.Notes)
        .WithName("GetNote")
        .WithDescription("Get a note by its unique identifier.")
        .WithSummary("Get a note by its unique identifier.");
    }
}
