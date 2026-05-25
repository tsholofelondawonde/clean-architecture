namespace clean_architecture.contracts.Notes;

public sealed record NoteResponse(
    Guid Id,
    string? Title,
    string? Content,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    bool IsDeleted
);