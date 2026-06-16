using clean_architecture.application.Abstractions.Messaging;

namespace clean_architecture.application.Features.Notes.GetById;

public sealed record GetNoteByIdQuery(Guid Id) : IQuery<GetNoteByIdResponse>;
