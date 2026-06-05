namespace clean_architecture.contracts.Notes;

public sealed class CreateNoteRequest
{
    public string? Title { get; init; }
    public string? Content { get; init; }
}
