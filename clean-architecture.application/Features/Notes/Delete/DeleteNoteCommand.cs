using clean_architecture.application.Abstractions.Messaging;

namespace clean_architecture.application.Features.Notes.Delete;

public sealed record DeleteNoteCommand(Guid Id) : ICommand<bool>;
