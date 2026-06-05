using clean_architecture.application.Abstractions.Messaging;
using clean_architecture.application.Notes.Update;
using clean_architecture.contracts.Notes;
using clean_architecture.WebApi.Extensions;
using clean_architecture.WebApi.Infrastructure;

namespace clean_architecture.WebApi.Endpoints.Notes;

internal sealed class Update : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/notes/{id:guid}", async (Guid id, UpdateNoteRequest request,
            ICommandHandler<UpdateNoteCommand, NoteResponse> handler,
            CancellationToken cancellation) =>
        {
            if (id == Guid.Empty)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    { "id", new[] { "A valid note ID is required." } }
                });
            }

            if (request is null)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    { "Request", new[] { "Request cannot be null." } }
                });
            }

            var command = new UpdateNoteCommand
            {
                Id = id,
                Title = request.Title,
                Content = request.Content
            };

            var result = await handler.Handle(command, cancellation);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Notes)
        .WithName("UpdateNote")
        .WithDescription("Updates an existing note.")
        .WithSummary("Updates the title and/or content of a note by ID.")
        .Produces<NoteResponse>(StatusCodes.Status200OK);
    }
}
