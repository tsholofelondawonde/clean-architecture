using SharedKernel;

namespace clean_architecture.domain.NotesManagement.Exceptions;
public static class NoteErrors
{
    public static readonly Error NullRequest = new("Note.NullRequest", "The request cannot be null.", ErrorType.Validation, "A valid note is required");

    public static readonly Error NotFound = new(
        "Note.Notfound",
        "The note was not found.",
        ErrorType.NotFound,
        "The requested note does not exist in the system."
    );

    public static readonly Error InvalidTitle = new(
        "Note.InvalidTitle",
        "The note title is invalid.",
        ErrorType.Validation,
        "Title cannot be empty or exceed maximum length."
    );

    public static readonly Error InvalidContent = new(
        "Note.InvalidContent",
        "The note content is invalid.",
        ErrorType.Validation,
        "Content cannot be empty or exceed maximum length."
    );

    public static readonly Error AlreadyDeleted = new(
        "Note.AlreadyDeleted",
        "The note is already deleted.",
        ErrorType.Validation,
        "Cannot perform this operation on a deleted note."
    );
}
