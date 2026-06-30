using clean_architecture.application.Abstractions.Data;
using clean_architecture.application.Abstractions.Messaging;
using clean_architecture.domain.NotesManagement.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace clean_architecture.application.Features.Notes.GetById;

internal sealed class GetNoteByIdQueryHandler(IApplicationDbContext context,
    ILogger<GetNoteByIdQueryHandler> logger) : IQueryHandler<GetNoteByIdQuery, GetNoteByIdResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ILogger<GetNoteByIdQueryHandler> _logger = logger;

    public async Task<Result<GetNoteByIdResponse>> Handle(GetNoteByIdQuery query, CancellationToken cancellationToken)
    {
        var noteResponse = await _context.Notes
            .Where(n => !n.IsDeleted && n.Id == query.Id)
            .AsNoTracking()
            .Select(n => new GetNoteByIdResponse(
                n.Id,
                n.Title.Value,
                n.Content.Value,
                n.CreatedAt,
                n.UpdatedAt,
                n.IsDeleted
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (noteResponse == null)
        {
            _logger.LogWarning("Note with ID {NoteId} not found.", query.Id);
            return Result.Failure<GetNoteByIdResponse>(NoteErrors.NotFound);
        }

        return Result.Success(noteResponse);
    }
}
