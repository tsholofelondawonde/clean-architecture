using clean_architecture.application.Abstractions.Messaging;
using clean_architecture.application.Features.Notes;

namespace clean_architecture.application.Features.Notes.Get;

public sealed record GetNotesQuery : IQuery<List<GetNotesResponse>>, ICachedQuery
{
    public string CacheKey => "notes:all";

    public TimeSpan? Expiration => null;

    public IReadOnlyCollection<string> Tags => [NotesCacheKeys.NotesTag];
}
