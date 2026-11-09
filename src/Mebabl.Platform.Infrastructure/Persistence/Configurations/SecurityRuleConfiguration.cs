using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mebabl.Platform.Domain.Entities.Database;

namespace Mebabl.Platform.Infrastructure.Persistence.Configurations;

/// <summary>
/// Persists the current published security policy for a project-owned collection.
/// The composite collection relationship enforces project isolation at the database
/// level instead of relying only on application-side checks.
/// </summary>
public sealed class SecurityRuleConfiguration
    : IEntityTypeConfiguration<SecurityRule>
{
    public void Configure(EntityTypeBuilder<SecurityRule> builder)
    {
        builder.ToTable(
            "SecurityRules",
            table =>
            {
                table.HasCheckConstraint(
                    "CK_SecurityRules_Revision_Positive",
                    "\"Revision\" > 0");

                table.HasCheckConstraint(
                    "CK_SecurityRules_Definition_Object",
                    "jsonb_typeof(\"Definition\") = 'object'");
            });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProjectId)
            .IsRequired();

        builder.Property(x => x.CollectionId)
            .IsRequired();

        builder.Property(x => x.Definition)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.Revision)
            .IsRequired()
            .HasDefaultValue(1)
            .IsConcurrencyToken();

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.PublishedAt);

        // One current policy is allowed for each collection in its project.
        // The project-leading index also supports project-scoped policy lookups.
        builder.HasIndex(x => new
            {
                x.ProjectId,
                x.CollectionId
            })
            .IsUnique()
            .HasDatabaseName("UX_SecurityRules_Project_Collection");

        builder.HasIndex(x => new
            {
                x.ProjectId,
                x.IsActive,
                x.PublishedAt
            })
            .HasDatabaseName("IX_SecurityRules_Project_Active_PublishedAt");

        builder.HasOne(x => x.Project)
            .WithMany()
            .HasForeignKey(x => x.ProjectId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        // A policy cannot reference a collection belonging to another project.
        // Physical collection deletion removes its policy; normal collection
        // lifecycle operations should use soft deletion.
        builder.HasOne(x => x.Collection)
            .WithMany()
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
            .OnDelete(DeleteBehavior.Cascade);
    }
}
