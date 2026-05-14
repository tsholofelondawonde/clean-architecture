using SharedKernel;

namespace clean_architecture.domain.NotesManagement.Exceptions;
public static class NoteErrors
{
    public static readonly Error NullRequest = new("Note.NullRequest", "The request cannot be null.",ErrorType.Validation,"A valid note is required");

    public static readonly Error NotFound = new(
        "Note.Notfound",
        "The note was not found.",
        ErrorType.NotFound,
        "The requested note does not exist in the system."
    );
}
