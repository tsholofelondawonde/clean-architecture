namespace clean_architecture.application.Features.Notes.Update;

public sealed record UpdateNoteResponse(
    Guid Id,
    string? Title,
    string? Content,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    bool IsDeleted
);
