using clean_architecture.application.Abstractions.Messaging;
using clean_architecture.application.Features.Notes.Delete;
using clean_architecture.WebApi.Extensions;
using clean_architecture.WebApi.Infrastructure;

namespace clean_architecture.WebApi.Endpoints.Notes;

internal sealed class Delete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/notes/{id:guid}", async (Guid id, ICommandHandler<DeleteNoteCommand, bool> handler, CancellationToken cancellation) =>
        {
            if (id == Guid.Empty)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    { "id", new[] { "A valid note ID is required." } }
                });
            }

            var command = new DeleteNoteCommand(id);
            var result = await handler.Handle(command, cancellation);

            return result.Match(_ => Results.NoContent(), CustomResults.Problem);
        })
        .WithTags(Tags.Notes)
        .WithName("DeleteNote")
        .WithDescription("Deletes a note by its unique identifier.")
        .WithSummary("Deletes a note.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}
