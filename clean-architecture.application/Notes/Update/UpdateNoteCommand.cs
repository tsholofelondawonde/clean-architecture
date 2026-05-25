using clean_architecture.application.Abstractions.Messaging;
using clean_architecture.contracts.Notes;

namespace clean_architecture.application.Notes.Update;

public sealed class UpdateNoteCommand : ICommand<NoteResponse>
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
}
