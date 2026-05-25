using clean_architecture.application.Abstractions.Messaging;
using clean_architecture.contracts.Notes;

namespace clean_architecture.application.Notes.GetById;

public sealed record GetNoteByIdQuery(Guid Id) : IQuery<NoteResponse>;

