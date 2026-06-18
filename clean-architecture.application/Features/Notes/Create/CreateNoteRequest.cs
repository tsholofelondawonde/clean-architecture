namespace clean_architecture.application.Features.Notes.Create;

public sealed class CreateNoteRequest
{
    public string? Title { get; init; }
    public string? Content { get; init; }
}
