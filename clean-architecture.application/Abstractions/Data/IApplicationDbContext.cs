using clean_architecture.domain.NotesManagement.Aggregates;

namespace clean_architecture.application.Abstractions.Data;

public interface IApplicationDbContext
{
    public DbSet<Note> Notes { get; set; }

    /// <summary>
    /// Provides access to the change tracker for managing entity state.
    /// Used for advanced scenarios like clearing tracked entities during retry operations.
    /// </summary>
    ChangeTracker ChangeTracker { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
