using SharedKernel;
using clean_architecture.domain.NotesManagement.Exceptions;

namespace clean_architecture.domain.NotesManagement.ValueObjects;

public sealed class NoteContent : ValueObject
{
    public const int MaxLength = 1000;
    public string Value { get; }

    private NoteContent(string value)
    {
        Value = value;
    }

    public static Result<NoteContent> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<NoteContent>(NoteErrors.InvalidContent);
        if (value.Length > MaxLength)
            return Result.Failure<NoteContent>(NoteErrors.InvalidContent);
        return Result.Success(new NoteContent(value));
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
