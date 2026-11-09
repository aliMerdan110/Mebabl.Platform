using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Entities.Database;

namespace Mebabl.Platform.Infrastructure.Persistence.Configurations;

public sealed class CollectionConfiguration
    : IEntityTypeConfiguration<Collection>
{
    public void Configure(EntityTypeBuilder<Collection> builder)
    {
        builder.ToTable("Collections");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProjectId)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Code)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.IsSystem)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        // Code is the normalized, stable collection identifier within a project.
        // A filtered unique index allows a soft-deleted collection's code to be reused.
        builder.HasIndex(x => new
            {
                x.ProjectId,
                x.Code
            })
            .IsUnique()
            .HasDatabaseName("UX_Collections_ProjectId_Code_Active")
            .HasFilter("\"IsDeleted\" = false");

        // Supports project-scoped collection-name lookup without relying on
        // application IDs, since applications in the same project share data.
        builder.HasIndex(x => new
            {
                x.ProjectId,
                x.Name
            })
            .HasDatabaseName("IX_Collections_ProjectId_Name_Active")
            .HasFilter("\"IsDeleted\" = false");

        builder.HasOne(x => x.Project)
            .WithMany()
            .HasForeignKey(x => x.ProjectId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}
