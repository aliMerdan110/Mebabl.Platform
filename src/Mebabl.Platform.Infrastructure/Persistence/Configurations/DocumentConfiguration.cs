using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Entities.Database;
using Mebabl.Platform.Domain.Entities.Identity;

namespace Mebabl.Platform.Infrastructure.Persistence.Configurations;

public sealed class DocumentConfiguration
    : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable(
            "Documents",
            table => table.HasCheckConstraint(
                "CK_Documents_Version_Positive",
                "\"Version\" >= 1"));

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProjectId)
            .IsRequired();

        builder.Property(x => x.CollectionId)
            .IsRequired();

        builder.Property(x => x.CreatedByApplicationId)
            .IsRequired();

        builder.Property(x => x.CreatedByUserId);

        builder.Property(x => x.Key)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Data)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.Version)
            .IsRequired()
            .IsConcurrencyToken();

        builder.Property(x => x.IsDeleted)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // A document key is unique within an active collection in a project.
        // Soft-deleted documents do not prevent a key from being reused.
        builder.HasIndex(x => new
            {
                x.ProjectId,
                x.CollectionId,
                x.Key
            })
            .IsUnique()
            .HasDatabaseName("UX_Documents_Project_Collection_Key_Active")
            .HasFilter("\"IsDeleted\" = false");

        // Supports the default chronological document listing and stable
        // cursor pagination using CreatedAt followed by the document ID.
        builder.HasIndex(x => new
            {
                x.ProjectId,
                x.CollectionId,
                x.CreatedAt,
                x.Id
            })
            .IsDescending(false, false, true, true)
            .HasDatabaseName("IX_Documents_Project_Collection_CreatedAt_Id_Active")
            .HasFilter("\"IsDeleted\" = false");

        // Supports owner-scoped document operations without treating the
        // creator as the sole authorization mechanism.
        builder.HasIndex(x => new
            {
                x.ProjectId,
                x.CollectionId,
                x.CreatedByUserId,
                x.CreatedAt,
                x.Id
            })
            .IsDescending(false, false, false, true, true)
            .HasDatabaseName("IX_Documents_Project_Collection_Creator_CreatedAt_Id_Active")
            .HasFilter("\"IsDeleted\" = false");

        // Documents are project-owned. The composite collection foreign key
        // prevents a document from referencing a collection in another project.
        builder.HasOne(x => x.Project)
            .WithMany()
            .HasForeignKey(x => x.ProjectId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Collection)
            .WithMany(x => x.Documents)
            .HasForeignKey(x => new
            {
                x.ProjectId,
                x.CollectionId
            })
            .HasPrincipalKey(x => new
            {
                x.ProjectId,
                x.Id
            })
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        // Provenance is constrained to an application registered in the same
        // project. This relation does not grant that application data access.
        builder.HasOne<PlatformApplication>()
            .WithMany()
            .HasForeignKey(x => new
            {
                x.ProjectId,
                x.CreatedByApplicationId
            })
            .HasPrincipalKey(x => new
            {
                x.ProjectId,
                x.Id
            })
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}
