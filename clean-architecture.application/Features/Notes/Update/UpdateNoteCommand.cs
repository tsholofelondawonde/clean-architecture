using clean_architecture.application.Abstractions.Messaging;

namespace clean_architecture.application.Features.Notes.Update;

public sealed class UpdateNoteCommand : ICommand<UpdateNoteResponse>
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
}
