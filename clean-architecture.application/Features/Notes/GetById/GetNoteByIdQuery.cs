using clean_architecture.application.Abstractions.Messaging;
using clean_architecture.application.Features.Notes;

namespace clean_architecture.application.Features.Notes.GetById;

public sealed record GetNoteByIdQuery(Guid Id) : IQuery<GetNoteByIdResponse>, ICachedQuery
{
    public string CacheKey => $"notes:{Id}";

    public TimeSpan? Expiration => null;

    public IReadOnlyCollection<string> Tags => [NotesCacheKeys.NotesTag];
}
