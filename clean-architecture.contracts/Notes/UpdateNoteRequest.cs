namespace clean_architecture.contracts.Notes;

public sealed class UpdateNoteRequest
{
    public string? Title { get; init; }
    public string? Content { get; init; }
}
