using clean_architecture.application.Abstractions.Data;
using clean_architecture.application.Abstractions.Messaging;
using clean_architecture.contracts.Notes;
using clean_architecture.domain.NotesManagement.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace clean_architecture.application.Notes.Update;

internal sealed class UpdateNoteCommandHandler(IApplicationDbContext context
    , ILogger<UpdateNoteCommandHandler> logger) : ICommandHandler<UpdateNoteCommand, NoteResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ILogger<UpdateNoteCommandHandler> _logger = logger;

    public async Task<Result<NoteResponse>> Handle(UpdateNoteCommand command, CancellationToken cancellationToken)
    {
		try
		{
			var note = await _context.Notes
				.FirstOrDefaultAsync(n => n.Id == command.Id, cancellationToken);

			if(note == null)
			{
				_logger.LogWarning("Note with ID {NoteId} not found for update.", command.Id);
				return Result.Failure<NoteResponse>(NoteErrors.NotFound);
            }

			var updateResult = note.Update(command.Title, command.Content);
			
			_context.Notes.Update(note);

			var savedResult = await _context.SaveChangesAsync(cancellationToken);

            if (savedResult == 0)
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
		catch (Exception)
		{
			throw;
		}
    }
}
