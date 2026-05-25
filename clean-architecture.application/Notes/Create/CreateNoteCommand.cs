using clean_architecture.application.Abstractions.Messaging;
using clean_architecture.contracts.Notes;

namespace clean_architecture.application.Notes.Create;

public sealed class CreateNoteCommand : ICommand<NoteResponse>
{
    public string? Title { get; set; }
    public string? Content { get; set; }
}
