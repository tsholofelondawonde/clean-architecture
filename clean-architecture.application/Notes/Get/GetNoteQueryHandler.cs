using clean_architecture.application.Abstractions.Data;
using clean_architecture.application.Abstractions.Messaging;
using clean_architecture.contracts.Notes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace clean_architecture.application.Notes.Get;

internal sealed class GetNoteQueryHandler(IApplicationDbContext context, ILogger<GetNoteQueryHandler> logger) : IQueryHandler<GetNoteQuery, List<NoteResponse>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ILogger<GetNoteQueryHandler> _logger = logger;

    public async Task<Result<List<NoteResponse>>> Handle(GetNoteQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var noteResponse = await _context.Notes
                .Where(n => !n.IsDeleted)
                .AsNoTracking()
                .Select(n => new NoteResponse(
                    n.Id,
                    n.Title.Value,
                    n.Content.Value,
                    n.CreatedAt,
                    n.UpdatedAt,
                    n.IsDeleted
                ))
               .ToListAsync(cancellationToken);

            return Result.Success(noteResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while getting notes.");
            return Result.Failure<List<NoteResponse>>(
                Error.Problem("Note.GetFailed", "An unexpected error occurred while getting notes."));
        }
    }
}
