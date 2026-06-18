using clean_architecture.application.Abstractions.Data;
using clean_architecture.application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace clean_architecture.application.Features.Notes.Get;

internal sealed class GetNotesQueryHandler(IApplicationDbContext context, ILogger<GetNotesQueryHandler> logger) : IQueryHandler<GetNotesQuery, List<GetNotesResponse>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ILogger<GetNotesQueryHandler> _logger = logger;

    public async Task<Result<List<GetNotesResponse>>> Handle(GetNotesQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var noteResponse = await _context.Notes
                .Where(n => !n.IsDeleted)
                .AsNoTracking()
                .Select(n => new GetNotesResponse(
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
            return Result.Failure<List<GetNotesResponse>>(
                Error.Problem("Note.GetFailed", "An unexpected error occurred while getting notes."));
        }
    }
}
