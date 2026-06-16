namespace clean_architecture.application.Features.Notes.GetById;

public sealed record GetNoteByIdResponse(
    Guid Id,
    string? Title,
    string? Content,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    bool IsDeleted
);
