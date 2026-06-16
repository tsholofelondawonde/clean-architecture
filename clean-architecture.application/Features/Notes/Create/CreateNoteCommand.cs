using clean_architecture.application.Abstractions.Messaging;

namespace clean_architecture.application.Features.Notes.Create;

public sealed class CreateNoteCommand : ICommand<CreateNoteResponse>
{
    public string? Title { get; set; }
    public string? Content { get; set; }
}
