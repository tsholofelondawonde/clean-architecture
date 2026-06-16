namespace clean_architecture.application.Features.Notes.Update;

public sealed class UpdateNoteRequest
{
    public string? Title { get; init; }
    public string? Content { get; init; }
}
