using SharedKernel;
using clean_architecture.domain.NotesManagement.Exceptions;

namespace clean_architecture.domain.NotesManagement.ValueObjects;

public sealed class NoteTitle : ValueObject
{
    public const int MaxLength = 100;
    public string Value { get; }

    private NoteTitle(string value)
    {
        Value = value;
    }

    public static Result<NoteTitle> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<NoteTitle>(NoteErrors.InvalidTitle);
        if (value.Length > MaxLength)
            return Result.Failure<NoteTitle>(NoteErrors.InvalidTitle);
        return Result.Success(new NoteTitle(value));
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
