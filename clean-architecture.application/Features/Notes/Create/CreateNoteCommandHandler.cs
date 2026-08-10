using clean_architecture.application.Abstractions.Data;
using clean_architecture.application.Abstractions.Messaging;
using clean_architecture.application.Features.Notes;
using clean_architecture.domain.NotesManagement.Aggregates;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace clean_architecture.application.Features.Notes.Create;

public sealed class CreateNoteCommandHandler(
    IApplicationDbContext context,
    HybridCache cache,
    ILogger<CreateNoteCommandHandler> logger) : ICommandHandler<CreateNoteCommand, CreateNoteResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;
    private readonly ILogger<CreateNoteCommandHandler> _logger = logger;

    public async Task<Result<CreateNoteResponse>> Handle(CreateNoteCommand command, CancellationToken cancellationToken)
    {
		var noteResult = Note.Create(command.Title, command.Content);

        if (noteResult.IsFailure)
        {
            _logger.LogWarning("Failed to create note: {ErrorMessage}", noteResult.Error);
            return Result<CreateNoteResponse>.ValidationFailure(noteResult.Error);
        }

        var note = noteResult.Value;

        _context.Notes.Add(note);

        var savedCount = await _context.SaveChangesAsync(cancellationToken);

        if (savedCount == 0)
        {
            _logger.LogError("Failed to save the new note to the database.");
            return Result<CreateNoteResponse>.ValidationFailure(
                Error.Failure("Note.SaveFailed", "Failed to save the new note to the database."));
        }

        var response = new CreateNoteResponse(
            note.Id,
            note.Title.Value,
            note.Content.Value,
            note.CreatedAt,
            note.UpdatedAt,
            note.IsDeleted);

        await _cache.RemoveByTagAsync(NotesCacheKeys.NotesTag, cancellationToken);

        return Result.Success(response);
    }
}
