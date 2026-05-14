using clean_architecture.domain.NotesManagement.DomainEvents;
using clean_architecture.domain.NotesManagement.Exceptions;
using clean_architecture.domain.NotesManagement.ValueObjects;
using SharedKernel;

namespace clean_architecture.domain.NotesManagement.Aggregates;

public class Note : Entity
{
	public NoteTitle Title { get; private set; } = null!;
	public NoteContent Content { get; private set; } = null!;
	public DateTime CreatedAt { get; private set; }
	public DateTime UpdatedAt { get; private set; }
	public bool IsDeleted { get; private set; }

	private Note() { }

	private Note(NoteTitle title, NoteContent content)
	{
		Title = title ?? throw new ArgumentNullException(nameof(title));
		Content = content ?? throw new ArgumentNullException(nameof(content));
		CreatedAt = DateTime.UtcNow;
		UpdatedAt = CreatedAt;
		IsDeleted = false;
	}

	/// <summary>
	/// Factory method to create a new note
	/// </summary>
	public static Result<Note> Create(string? titleValue, string? contentValue)
	{
		var titleResult = NoteTitle.Create(titleValue);
		if (titleResult.IsFailure)
			return Result.Failure<Note>(titleResult.Error);

		var contentResult = NoteContent.Create(contentValue);
		if (contentResult.IsFailure)
			return Result.Failure<Note>(contentResult.Error);

		var note = new Note(titleResult.Value, contentResult.Value);
		note.Raise(new NoteCreatedDomainEvent(note));

		return Result.Success(note);
	}

	/// <summary>
	/// Updates the note with new title and content
	/// </summary>
	public Result Update(string? titleValue, string? contentValue)
	{
		if (IsDeleted)
			return Result.Failure(NoteErrors.AlreadyDeleted);

		var titleResult = NoteTitle.Create(titleValue);
		if (titleResult.IsFailure)
			return Result.Failure(titleResult.Error);

		var contentResult = NoteContent.Create(contentValue);
		if (contentResult.IsFailure)
			return Result.Failure(contentResult.Error);

		Title = titleResult.Value;
		Content = contentResult.Value;
		UpdatedAt = DateTime.UtcNow;

		Raise(new NoteUpdateDomainEvent(this));

		return Result.Success();
	}

	/// <summary>
	/// Soft deletes the note
	/// </summary>
	public Result SoftDelete()
	{
		if (IsDeleted)
			return Result.Failure(NoteErrors.AlreadyDeleted);

		IsDeleted = true;
		UpdatedAt = DateTime.UtcNow;

		Raise(new NoteDeletedDomainEvent(this));

		return Result.Success();
	}
}
