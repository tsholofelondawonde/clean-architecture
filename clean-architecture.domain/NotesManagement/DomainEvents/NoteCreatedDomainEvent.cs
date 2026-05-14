using clean_architecture.domain.NotesManagement.Aggregates;
using SharedKernel;

namespace clean_architecture.domain.NotesManagement.DomainEvents;
public sealed record NoteCreatedDomainEvent(Note Note) : IDomainEvent;
