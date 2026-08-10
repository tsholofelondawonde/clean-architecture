namespace clean_architecture.application.Features.Notes;

/// <summary>
/// Cache key/tag conventions shared by cached Notes queries and the command handlers
/// that invalidate them on write.
/// </summary>
internal static class NotesCacheKeys
{
    internal const string NotesTag = "notes";
}
