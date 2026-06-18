using clean_architecture.domain.NotesManagement.Aggregates;
using clean_architecture.domain.NotesManagement.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace clean_architecture.infrastructure.Database.Configurations;

internal sealed class NoteConfiguration : IEntityTypeConfiguration<Note>
{
    public void Configure(EntityTypeBuilder<Note> builder)
    {
        builder.HasKey(n => n.Id);

        builder.Property(n => n.Title)
            .HasConversion(
                title => title.Value,
                value => NoteTitle.Create(value).Value)
            .HasMaxLength(NoteTitle.MaxLength)
            .IsRequired();

        builder.Property(n => n.Content)
            .HasConversion(
                content => content.Value,
                value => NoteContent.Create(value).Value)
            .HasMaxLength(NoteContent.MaxLength)
            .IsRequired();

        builder.Property(n => n.CreatedAt).IsRequired();
        builder.Property(n => n.UpdatedAt).IsRequired();
        builder.Property(n => n.IsDeleted).IsRequired();
    }
}
