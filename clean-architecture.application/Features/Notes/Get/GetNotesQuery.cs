using clean_architecture.application.Abstractions.Messaging;

namespace clean_architecture.application.Features.Notes.Get;

public sealed record GetNotesQuery : IQuery<List<GetNotesResponse>>;
