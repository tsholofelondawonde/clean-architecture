using clean_architecture.application.Abstractions.Data;
using clean_architecture.application.Abstractions.Messaging;
using clean_architecture.contracts.Notes;
using clean_architecture.domain.NotesManagement.Aggregates;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace clean_architecture.application.Notes.Create;

public sealed class CreateNoteCommandHandler(IApplicationDbContext context, ILogger<CreateNoteCommandHandler> logger) : ICommandHandler<CreateNoteCommand, NoteResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ILogger<CreateNoteCommandHandler> _logger = logger;

    public async Task<Result<NoteResponse>> Handle(CreateNoteCommand command, CancellationToken cancellationToken)
    {
		var noteResult = Note.Create(command.Title, command.Content);

        if (noteResult.IsFailure)
        {
            _logger.LogWarning("Failed to create note: {ErrorMessage}", noteResult.Error);
            return Result<NoteResponse>.ValidationFailure(noteResult.Error);
        }

        var note = noteResult.Value;

        _context.Notes.Add(note);

        var savedCount = await _context.SaveChangesAsync(cancellationToken);

        if (savedCount == 0)
        {
            _logger.LogError("Failed to save the new note to the database.");
            return Result<NoteResponse>.ValidationFailure(
                Error.Failure("Note.SaveFailed", "Failed to save the new note to the database."));
        }

        var response = new NoteResponse(
            note.Id,
            note.Title.Value,
            note.Content.Value,
            note.CreatedAt,
            note.UpdatedAt,
            note.IsDeleted);

        return Result.Success(response);
    }
}
