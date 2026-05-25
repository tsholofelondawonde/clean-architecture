using clean_architecture.application.Abstractions.Messaging;
using clean_architecture.contracts.Notes;

namespace clean_architecture.application.Notes.Get;

public sealed record GetNoteQuery : IQuery<List<NoteResponse>>;
