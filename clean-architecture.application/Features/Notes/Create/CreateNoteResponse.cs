namespace clean_architecture.application.Features.Notes.Create;

public sealed record CreateNoteResponse(
    Guid Id,
    string? Title,
    string? Content,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    bool IsDeleted
);
