using clean_architecture.application.Abstractions.Messaging;
using clean_architecture.application.Features.Notes.Create;
using clean_architecture.WebApi.Extensions;
using clean_architecture.WebApi.Infrastructure;

namespace clean_architecture.WebApi.Endpoints.Notes;

internal sealed class Create : IEndpoint
{

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/notes", async (CreateNoteRequest request, ICommandHandler<CreateNoteCommand, CreateNoteResponse> handler
            , CancellationToken cancellation) =>
        {
            if (request is null)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    { "Request", new[] { "Request cannot be null." } }
                });
            }

            var command = new CreateNoteCommand
            {
                Title = request.Title,
                Content = request.Content
            };

            var result = await handler.Handle(command, cancellation);
             
            return result.Match(Results.Ok, CustomResults.Problem);

        })
        .WithTags(Tags.Notes)    
        .WithName("CreateNote")
        .WithDescription("Creates a new note.")
        .WithSummary("Creates a new note with the provided title and content.")
        .Produces<CreateNoteResponse>(StatusCodes.Status201Created);
    }
}
