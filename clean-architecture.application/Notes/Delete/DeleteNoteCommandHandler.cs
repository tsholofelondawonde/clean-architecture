using clean_architecture.application.Abstractions.Data;
using clean_architecture.application.Abstractions.Messaging;
using clean_architecture.domain.NotesManagement.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace clean_architecture.application.Notes.Delete;

internal sealed class DeleteNoteCommandHandler(IApplicationDbContext context,
    ILogger<DeleteNoteCommandHandler> logger) : ICommandHandler<DeleteNoteCommand, bool>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ILogger<DeleteNoteCommandHandler> _logger = logger;

    public async Task<Result<bool>> Handle(DeleteNoteCommand command, CancellationToken cancellationToken)
    {
		try
		{
			var note = await _context.Notes
                .FirstOrDefaultAsync(c => c.Id == command.Id && !c.IsDeleted, cancellationToken);

            if(note == null)
            {
                _logger.LogWarning("Note with ID {NoteId} not found for deletion.", command.Id);
                return Result.Failure<bool>(NoteErrors.NotFound);
            }

            var deleted = note.SoftDelete();
            if(deleted.IsFailure)
            {
                _logger.LogWarning("Note with ID {NoteId} is already deleted.", command.Id);
                return Result.Failure<bool>(NoteErrors.AlreadyDeleted);
            }

            _context.Notes.Update(note);
            var savedCount = await _context.SaveChangesAsync(cancellationToken);

            if (savedCount == 0)
            {
                _logger.LogError("Failed to delete note with ID {NoteId}. No changes were saved to the database.", command.Id);
                return Result.Failure<bool>(new Error("Note.DeletionFailed", "Failed to delete the note due to an unknown error.", ErrorType.Failure));
            }

            return Result.Success(true);
        }
		catch (Exception)
		{
			throw;
		}
    }
}
