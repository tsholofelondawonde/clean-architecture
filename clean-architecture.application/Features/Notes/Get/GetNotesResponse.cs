namespace clean_architecture.application.Features.Notes.Get;

public sealed record GetNotesResponse(
    Guid Id,
    string? Title,
    string? Content,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    bool IsDeleted
);
