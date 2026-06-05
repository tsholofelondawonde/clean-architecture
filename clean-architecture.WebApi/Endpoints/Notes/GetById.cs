using clean_architecture.application.Abstractions.Messaging;
using clean_architecture.application.Notes.GetById;
using clean_architecture.contracts.Notes;
using clean_architecture.WebApi.Extensions;

namespace clean_architecture.WebApi.Endpoints.Notes;

internal sealed class GetById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/notes/{id:guid}", async (Guid id, IQueryHandler<GetNoteByIdQuery, NoteResponse> handler, CancellationToken cancellation) =>
        {
            if (id == Guid.Empty)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    { "planId", new[] { "A valid note ID is required." } }
                });
            }

            var query = new GetNoteByIdQuery(id);
            var result = await handler.Handle(query, cancellation);
            return result.Match(Results.Ok, Results.NotFound);
        })
        .WithTags(Tags.Notes)
        .WithName("GetNotesById")
        .WithDescription("Gets a note by its unique identifier.")
        .WithSummary("Gets a note by its unique identifier.")
        .Produces<NoteResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
